using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using com.esp.common;
using nfc.felica;

namespace acr122u
{
    public enum NxpCmd : byte
    {
        GetFirmwareVersion = 0x02,

        SetParameters = 0x12,

        InJumpDEP = 0x56,
        InDataEx = 0x40,
        InAtr=0x50,
        InRelease = 0x52,
        InListPassiveTarget = 0x4A,


        TgInitAsTarget = 0x8C,
        TgGetData = 0x86,
        TgGetInCommand = 0x88,
        TgGetTargetStatus = 0x8A,
        TgSetData = 0x8E,
        TgResToIn = 0x90,
    }

    public enum BoudRate
    {
        SP_106 = 0,
        SP_212 = 1,
        SP_424 = 2,
    }

    public enum FramingType
    {
        Mifare = 0,
        ActiveMode = 1,
        Felica = 2,
    }

    public class Acr122u
    {
        private const int RECV_PREFIX_LEN = 3, RECV_SUFIX_LEN=2;

        private int context = 0, card=0;
        private string rwName;
        private byte[] sendBuff = new byte[256];
        private byte[] recvBuff = new byte[256];
        private int recvLen;

        public bool HasCard
        {
            get
            {
                if (card == 0)
                {
                    return false;
                }
                else
                {
                    byte status, rBit;
                    if (!TgGetTargetStatus(out status, out rBit))
                        return false;
                    return status == 0;
                }
            }
        }

        public bool Init()
        {
            if (context != 0)
                return true;

            int retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref context);
            int readers = 0;
            retCode = ModWinsCard.SCardListReaders(context, null, null, ref readers);
            if (readers < 1)
            {
                Debug.WriteLine("NoReader!");
                return false;
            }

            // Fill reader list
            byte[] nameList = new byte[readers];
            retCode = ModWinsCard.SCardListReaders(context, null, nameList, ref readers);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                return false;

            int len = 0;
            foreach (byte b in nameList)
            {
                if (b == 0)
                    break;
                len++;
            }

            //リーダー名称は\0繋ぎ
            rwName = Encoding.ASCII.GetString(nameList, 0, len);

            return true;
        }

        public void ReleaseContext()
        {
            if (context != 0)
            {
                ModWinsCard.SCardReleaseContext(context);
                Debug.WriteLine("Release Context");
                context = 0;
            }
        }

        public bool ConnectCard()
        {
            if (card != 0)
                return true;

            //uset first reader.
            int protocol = 0;
            int retCode = ModWinsCard.SCardConnect(context, rwName, ModWinsCard.SCARD_SHARE_DIRECT, ModWinsCard.SCARD_PROTOCOL_UNDEFINED, ref card, ref protocol);

            if (retCode == ModWinsCard.SCARD_S_SUCCESS)
            {
                Debug.WriteLine("Connectted!");
                return true;
            }
            else
            {
                Debug.WriteLine("Connect Card ERROR:" + ModWinsCard.GetScardErrMsg(retCode));
                return false;
            }
        }

        public void ReleaseCard()
        {
            if(card==0)
                return;

            Debug.WriteLine("Release Card");
            int retCode = ModWinsCard.SCardDisconnect(card, 1);
            card = 0;
        }

        public bool SetParams(byte param)
        {
            byte status;
            return DCommand(NxpCmd.SetParameters, new byte[] { param }, out status);
        }

        public void SetRfMode(byte param)
        {
            byte status;
            DCommand(NxpCmd.SetParameters, new byte[] { 0x00 }, out status);
        }

        #region TargetMode
        public byte[] TgReadData()
        {
            byte status=0;
            if(DCommand(NxpCmd.TgGetData, new byte[0], out status))
                return recvBuff;
            return null;
        }

        public byte[] TgGetInCommand()
        {
            byte status = 0;
            if (!DCommand(NxpCmd.TgGetInCommand, new byte[0], out status))
                return null;

            byte[] ret = new byte[recvLen - RECV_PREFIX_LEN - RECV_SUFIX_LEN];
            Array.Copy(recvBuff, RECV_PREFIX_LEN, ret, 0, ret.Length);
            return ret;
        }

        public bool TgResToIn(byte[] data)
        {
            byte status;
            return DCommand(NxpCmd.TgResToIn, data, out status);
        }

        public bool TgSetData(byte[] data)
        {
            byte status;
            return DCommand(NxpCmd.TgSetData, data, out status);
        }

        public bool TgGetTargetStatus(out byte status, out byte bRit)
        {
            status = bRit = 0;
            if (!DCommand(NxpCmd.TgGetInCommand, new byte[0], out status))
                return false;

            status = recvBuff[2];
            bRit = recvBuff[3];
            return true;
        }

        public bool TgMode(byte[] mifare, byte[] idm, byte[] pmm, ushort sysCode, byte[] nfcid3, byte[] gt, out byte mode, out byte[] inCmd)
        {
            mode = 0;
            inCmd = null;

            int index = 0;
            int gtLen = gt != null ? gt.Length : 0;
            byte[] cmd = new byte[1 + 6 + 18 + 10 + 1 + gtLen + 1];

            cmd[index++] = 0x01;//Passive

            if (mifare != null)
                mifare.CopyTo(cmd, index);
            index += 6;

            if (idm != null)
                idm.CopyTo(cmd, index);
            index += 8;

            if (pmm != null)
                pmm.CopyTo(cmd, index);
            index += 8;

            com.esp.common.EndianConverter.SetUIntToBytes((ushort)sysCode, cmd, index);
            index += 2;

            if(nfcid3!=null)
                nfcid3.CopyTo(cmd, index);
            index += 10;

            cmd[index++] = (byte)(gt!=null? gt.Length: 0);
            if (gt != null)
            {
                gt.CopyTo(cmd, index);
                index += gt.Length;
            }
            cmd[index++] = 0;

            byte status;
            if (!DCommand(NxpCmd.TgInitAsTarget, cmd, out status))
                return false;

            //D5 8D MODE
            mode = recvBuff[2];

            byte[] ret = new byte[recvLen - RECV_PREFIX_LEN - RECV_SUFIX_LEN];
            Array.Copy(recvBuff, RECV_PREFIX_LEN, ret, 0, ret.Length);
            inCmd = ret;
            return true;
        }

        #endregion

        #region InitiatorMode
        public byte[] FPolling(ushort systemCode)
        {
            byte[] cmd = new byte[1 + 1 + 5];
            int index = 0;

            cmd[index++] = 1;
            cmd[index++] = 0x01;
            cmd[index++] = 0x00;//Polling
            cmd[index++] = (byte)(systemCode>>8);
            cmd[index++] = (byte)(systemCode&0xff);
            cmd[index++] = 0;
            cmd[index++] = 0;
            byte status;
            if(!DCommand(NxpCmd.InListPassiveTarget, cmd, out status))
                return null;

            byte[] ret = new byte[Felica.IDM_LENGTH];
            //D5 4B DETECTED_COUNT {TAG_NUM LEN 0x01 IDM[8] PMM[8]}
            if (recvBuff[2] != 1)
                return null;//Not detect
            Array.Copy(recvBuff, 6, ret, 0, ret.Length);
            return ret;   
        }

        public bool InMode(bool active, BoudRate br, byte[] gi)
        {
            byte[] cmd = new byte[1 + 1+ 1+ (active? 0: 1) + gi.Length];
            int index = 0;

            cmd[index++] = (byte)(active ? 1 : 0);
            cmd[index++] = (byte)br;
            cmd[index++] = (byte)((gi != null) ? 0x04 : 0);
            
            if(!active)
                index += (br==BoudRate.SP_106? 4: 5);

            if (gi != null)
            {
                gi.CopyTo(cmd, index);
                index += gi.Length;
            }

            byte status;
            return DCommand(NxpCmd.InJumpDEP, cmd, out status);
        }

        public byte[] InSendData(byte[] data)
        {
            byte status;
            byte[] cmd = new byte[data.Length + 1];
            cmd[0] = 1;
            Array.Copy(data, 0, cmd, 1, data.Length);

            if (!DCommand(NxpCmd.InDataEx, cmd, out status))
                return null;

            byte[] ret = new byte[recvLen - RECV_PREFIX_LEN - RECV_SUFIX_LEN];
            Array.Copy(recvBuff, RECV_PREFIX_LEN, ret, 0, ret.Length);
            return ret;
        }

        public bool InRelease()
        {
            byte status;
            return DCommand(NxpCmd.InRelease, new byte[]{0}, out status);
        }
        #endregion
        
        public bool DCommand(NxpCmd cmd, byte[] body, out byte status)
        {
            status = 0;
            //Initialize ？
            if (context == 0)
                return false;

            if (card == 0)
                return false;

            //start
            byte[] header = new byte[] { 0xff, 0, 0, 0 };

            int index = 0;
            Array.Copy(header, 0, sendBuff, index, header.Length);
            index += header.Length;

            sendBuff[index++] = (byte)(body.Length + 2);//LEN = D4,CMD,BODY
            sendBuff[index++] = 0xd4;
            sendBuff[index++] = (byte)cmd;

            Array.Copy(body, 0, sendBuff, index, body.Length);
            index += body.Length;

            Debug.WriteLine("<<<" + Utility.ByteToHex(sendBuff, 0, index));
            int retCode = ModWinsCard.SCardControl(card, (int)ModWinsCard.IOCTL_CCID_ESCAPE_SCARD_CTL_CODE, ref sendBuff[0], index, ref recvBuff[0], recvBuff.Length, ref recvLen);
            if (retCode == ModWinsCard.SCARD_S_SUCCESS)
            {
                Debug.WriteLine(">>>" + Utility.ByteToHex(recvBuff, 0, recvLen));
            }
            else
            {
                Debug.WriteLine("Command ERROR:" + ModWinsCard.GetScardErrMsg(retCode));
                ReleaseContext();
                return false;
            }

            if (recvBuff[recvLen - 2] != 0x90 || recvBuff[recvLen - 1] != 00)
            {
                return false;
            }

            if (recvBuff[1] != (byte)cmd + 1)
            {
                ReleaseCard();
                return false;
            }


            switch (cmd)
            {
                case NxpCmd.GetFirmwareVersion:
                case NxpCmd.InListPassiveTarget:
                case NxpCmd.TgInitAsTarget:
                case NxpCmd.TgGetTargetStatus:
                case NxpCmd.SetParameters:
                    break;

                default:
                    if (recvBuff[2] != 0)
                    {
                        ReleaseCard();
                        return false;
                    }
                    break;
            }
            return true;
        }
    }
}
