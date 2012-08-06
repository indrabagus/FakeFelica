using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

using com.esp.common;
using nfc.ndef;

namespace nfc.felica
{
    public class FCardController
    {
        protected FakeFelica fFelica;
        protected ushort[] systemCode;
        protected byte[] idm;

        public FCardController(FakeFelica fFelica, byte[] idm)
        {
            systemCode = new ushort[] { 0xffff };
            this.fFelica = fFelica;
            this.idm = idm;
        }

        public virtual void Init()
        {
            fFelica.Idm = idm;
            fFelica.SystemCode = systemCode[0];
            fFelica.CommandReceived += new EventHandler<FelicaEventArgs>(fFelica_CommandReceived);
        }

        private void fFelica_CommandReceived(object sender, FelicaEventArgs e)
        {
            switch (e.Cmd)
            {
                case FCmd.Poll:
                    OnPolling(sender, e);
                    break;
                case FCmd.ReqSv:
                    break;
                case FCmd.ReadWE:
                    OnRead(sender, e);
                    break;
                case FCmd.WriteWE:
                    OnWrite(sender, e);
                    break;
                case FCmd.ReqSys:
                    OnReqSys(sender, e);
                    break;
            }
        }

        protected virtual void OnPolling(object sender, FelicaEventArgs e)
        {
            FPollEventArgs poll = e as FPollEventArgs;

            foreach (ushort sys in systemCode)
            {
                ushort maskSys=sys;
                for (int i = 0; i < 4; i++)
                {
                    ushort mask = (ushort)(0x000f << i * 4);
                    if ((poll.SystemCode&mask)==mask)
                    {
                        maskSys |= mask;
                    }
                }

                if ((maskSys & poll.SystemCode) == maskSys)
                {
                    fFelica.DoResponse(FCmd.PollRes, fFelica.Pmm);
                    return;
                }
            }
            Thread.Sleep(100);
        }

        protected virtual void OnRead(object sender, FelicaEventArgs e)
        {
            FReadEventArgs read = e as FReadEventArgs;
            int blockCount = read.Block.Count;
            
            int index = 0;
            byte[] res = new byte[2 + 1 + blockCount * Felica.BLOCK_LENGTH];//ST[2],BLOCK,DATA[16*BLOCK]
            res[index++] = 0;//ST
            res[index++] = 0;//ST
            fFelica.DoResponse(FCmd.ReadWERes, res);
        }

        protected virtual void OnWrite(object sender, FelicaEventArgs e)
        {
            FWriteEventArgs write = e as FWriteEventArgs;

            int index = 0;
            byte[] res = new byte[2];
            res[index++] = 0x00;//ST 0
            res[index++] = 0x00;//ST 0
            fFelica.DoResponse(FCmd.WriteWERes, res);
        }

        protected virtual void OnReqSys(object sender, FelicaEventArgs e)
        {
            int index = 0;
            byte[] res = new byte[1 + systemCode.Length * 2];
            res[index++] = 1;
            foreach (ushort sys in systemCode)
            {
                EndianConverter.SetUIntToBytes((ushort)sys, res, index);
                index += 2;
            }

            fFelica.DoResponse(FCmd.ReqSysRes, res);
        }

        protected void OnError(FakeFelica fFelica, FCmd cmd, byte st1, byte st2)
        {
            byte[] res = new byte[2];
            int index = 0;
            res[index++] = st1;//ST
            res[index++] = st2;//ST
            fFelica.DoResponse(cmd, res);
        }
    }
}
