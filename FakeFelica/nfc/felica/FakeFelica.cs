using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using com.esp.common;
using acr122u;

namespace nfc.felica
{
    public class FelicaEventArgs : EventArgs
    {
        protected FCmd cmd;
        public FCmd Cmd { get { return cmd; } }

        public FelicaEventArgs(FCmd cmd)
        {
            this.cmd = cmd;
        }
    }

    public class FPollEventArgs : FelicaEventArgs
    {
        public ushort SystemCode { get; private set; }

        public FPollEventArgs(ushort systemCode)
            : base(FCmd.Poll)
        {
            this.SystemCode = systemCode;
        }
    }

    public class FReadEventArgs : FelicaEventArgs
    {
        protected List<BlockElement> block;
        public List<BlockElement> Block { get { return block; } }

        public FReadEventArgs(List<BlockElement> block)
            :base(FCmd.ReadWE)
        {
            this.block = block;
        }
    }

    public class FWriteEventArgs : FelicaEventArgs
    {
        protected List<BlockElement> block;
        public List<BlockElement> Block { get { return block; } }

        protected byte[] data;
        public byte[] Data { get { return data; } }

        public FWriteEventArgs(List<BlockElement> block, byte[] data)
            : base(FCmd.WriteWE)
        {
            this.block = block;
            this.data = data;
        }
    }

    public class BlockElement
    {
        /// <summary>FeliCa SystemCode</summary>
        public ushort ServiceCode { get; set; }
        /// <summary>first block is zero</summary>
        public int Number { get; set; }
    }

    public enum FCmd : byte
    {
        Poll = 0x00, PollRes = 0x01,
        ReqSv = 0x02, ReqSvRes = 0x03,
        ReqRes = 0x04, ReqResRes = 0x05,
        ReadWE = 0x06, ReadWERes = 0x07,
        WriteWE = 0x08, WriteWERes = 0x09,
        ReqSys = 0x0c, ReqSysRes = 0x0d,
    }

    public class FakeFelica
    {
        private bool abort = false;
        private Acr122u rw;
        private Thread worker;

        public byte[] Idm { get; set; }
        public byte[] Pmm { get; set; }
        public ushort SystemCode { get; set; }

        public event EventHandler<FelicaEventArgs> CommandReceived;

        #region DefaultConstructor
        public FakeFelica(Acr122u rw)
        {
            this.rw = rw;

            Idm = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            //Mobile FeliCa Chip PMM         01 13 8b 42 8f be cb ff
            //Felica card PMM  03 01 4b 02 4f 49 93 ff
            Pmm = new byte[] { 0x01, 0x13, 0x8b, 0x42, 0x8f, 0xbe, 0xcb, 0xff };
            //Default all type match
            SystemCode = 0xffff;
        }
        #endregion

        #region Thread Control
        public void Start()
        {
            worker = new Thread(new ThreadStart(Worker));
            worker.Start();
        }

        public void Abort()
        {
            abort = true;
            worker.Join(5000);
        }

        public void Worker()
        {
            abort = false;
            while (!abort)
            {
                try
                {
                    Thread.Sleep(100);
                    if (!rw.Init())
                        return;

                    if (!rw.ConnectCard())
                        continue;

                    rw.SetParams(0);

                    byte[] cmd = SetTargetMode();
                    if (cmd == null)
                        continue;

                    CardSession(cmd);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            rw.ReleaseContext();
            rw.ReleaseCard();
        }

        private void CardSession(byte[] cmd)
        {
            do
            {
                if (cmd != null && cmd.Length != 0)
                {
                    FCmd fcmd = (FCmd)cmd[0];
                    Debug.WriteLine("Cmd " + fcmd.ToString());
                    switch (fcmd)
                    {
                        case FCmd.Poll:
                            onPolling(cmd);
                            break;
                        case FCmd.ReqSv:
                            onReqSv(cmd);
                            break;
                        case FCmd.ReqRes:
                            onReqRes(cmd);
                            break;
                        case FCmd.ReadWE:
                            onRead(cmd);
                            break;
                        case FCmd.WriteWE:
                            onWrite(cmd);
                            break;
                        case FCmd.ReqSys:
                            onReqSys(cmd);
                            break;
                    }
                }
                else
                {
                    if (!rw.HasCard)
                        break;
                }
                //loop
                cmd = WaitCmd();
            } while (!abort);
        }

        #endregion

        #region FeliCa Command-Response
        private void onPolling(byte[] cmd)
        {
            if (CommandReceived != null)
            {
                ushort systemCode = (ushort)EndianConverter.BytesToUInteger(cmd, 1, 2);
                FPollEventArgs e = new FPollEventArgs(systemCode);
                CommandReceived(this, e);
            }
        }

        private void onReqRes(byte[] cmd)
        {
            byte[] res = new byte[1];

            int index = 0;
            res[index++] = 0x00;//MODE 0

            DoResponse(FCmd.ReqResRes, res);
        }

        private void onRead(byte[] cmd)
        {
            int index = 9;

            //ServieCode List
            ushort[] svCode = new ushort[cmd[index++]];
            for (int i = 0; i < svCode.Length; i++)
            {
                svCode[i] = BitConverter.ToUInt16(cmd, index + i * 2);
            }
            index += svCode.Length * 2;

            //Block List
            int blockCount = cmd[index++];
            List<BlockElement> blockList = new List<BlockElement>();
            for (int i = 0; i < blockCount; i++)
            {
                BlockElement elm = GetBlockElement(cmd, index);
                elm.ServiceCode = svCode[elm.ServiceCode];//replace
                Debug.WriteLine(String.Format("SV:{0:X04}, BLOCK:{1}", elm.ServiceCode, elm.Number));
                blockList.Add(elm);
                index += 2;
            }

            if (CommandReceived != null)
            {
                FReadEventArgs e = new FReadEventArgs(blockList);
                CommandReceived(this, e);
            }
        }

        private void onWrite(byte[] cmd)
        {
            int index = 9;

            //ServieCode List
            ushort[] svCode = new ushort[cmd[index++]];
            for (int i = 0; i < svCode.Length; i++)
            {
                svCode[i] = BitConverter.ToUInt16(cmd, index + i * 2);
            }
            index += svCode.Length * 2;

            //Block List
            int blockCount = cmd[index++];
            List<BlockElement> blockList = new List<BlockElement>();
            for (int i = 0; i < blockCount; i++)
            {
                BlockElement elm = GetBlockElement(cmd, index);
                elm.ServiceCode = svCode[elm.ServiceCode];//replace
                Debug.WriteLine(String.Format("SV:{0:X04}, BLOCK:{1}", elm.ServiceCode, elm.Number));
                blockList.Add(elm);
                index += 2;
            }

            //Block Data List
            byte[] data = new byte[Felica.BLOCK_LENGTH * blockCount];
            Array.Copy(cmd, index, data, 0, data.Length);
            Debug.WriteLine(String.Format("DATA{0}:",Utility.ByteToHex(data,0,data.Length)));

            if (CommandReceived != null)
            {
                FWriteEventArgs e = new FWriteEventArgs(blockList, data);
                CommandReceived(this, e);
            }
        }

        private void onReqSv(byte[] cmd)
        {
            int index = 9;
            ushort[] svCode = new ushort[cmd[index++]];
            for (int i = 0; i < svCode.Length; i++)
            {
                svCode[i] = BitConverter.ToUInt16(cmd, index + i * 2);
            }
            index += svCode.Length * 2;

            index = 0;
            byte[] fcmd = new byte[1 + svCode.Length * 2];//COUNT,SV[16*BLOCK]

            fcmd[index++] = (byte)svCode.Length;

            for (int i = 0; i < svCode.Length; i++)
            {
                EndianConverter.SetUIntToBytes((ushort)0xffff, fcmd, index+i*2);
            }
            index += svCode.Length * 2;

            DoResponse(FCmd.ReadWERes, fcmd);
        }

        private void onReqSys(byte[] cmd)
        {
            if (CommandReceived != null)
            {
                FelicaEventArgs e = new FelicaEventArgs(FCmd.ReqSys);
                CommandReceived(this, e);
            }
        }

        public void DoResponse(FCmd cmd, byte[] body)
        {
            int index = 0;
            byte[] fdata = new byte[(byte)(1 + 1 + Idm.Length + body.Length)];
            fdata[index++] = (byte)(1 + 1 + Idm.Length + body.Length);//LEN CMD IDM BODY
            fdata[index++] = (byte)cmd;

            Idm.CopyTo(fdata, index);
            index += Idm.Length;

            body.CopyTo(fdata, index);
            index += body.Length;

            rw.TgResToIn(fdata);
        }
        #endregion

        #region Common
        private byte[] SetTargetMode()
        {
            byte mode;
            byte[] inCmd;

            byte[] mf = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0xff};
            if (!rw.TgMode(mf, Idm, Pmm, SystemCode, null, null, out mode, out inCmd))
                return null;

            if ((mode & 0x03) != (byte)FramingType.Felica)
                return null;

            if (inCmd.Length > 1)
            {
                byte[] felicaCmd = new byte[inCmd.Length - 1];//length
                Array.Copy(inCmd, 1, felicaCmd, 0, felicaCmd.Length);
                return felicaCmd;
            }
            else
            {
                return new byte[0];
            }
        }

        private byte[] WaitCmd()
        {
            byte[] inCmd = rw.TgGetInCommand();
            if (inCmd == null || inCmd.Length==0)
                return null;

            byte[] ret = new byte[inCmd.Length-1];
            Array.Copy(inCmd, 1, ret, 0, ret.Length);
            return ret;
        }

        private BlockElement GetBlockElement(byte[] buf, int start)
        {
            BlockElement elm = new BlockElement();
            if ((buf[start] & 0x80) != 0)
            {
                //2byte style BlockElement
                byte acMode =     (byte)((buf[start] >> 4) & 0x07);
                elm.ServiceCode = (byte)(buf[start] & 0x0f);//this number is Temporally and replaced to real service code.
                elm.Number = buf[start + 1];
            }
            else
            {
                //3byte style BlockElement
                byte acMode = (byte)((buf[start] >> 4) & 0x07);
                elm.ServiceCode = (byte)(buf[start] & 0x0f);//this number is Temporally and replaced to real service code.
                elm.Number = (ushort)EndianConverter.BytesToUInteger(buf, start+1, 2);
            }
            return elm;
        }
        #endregion
    }
}
