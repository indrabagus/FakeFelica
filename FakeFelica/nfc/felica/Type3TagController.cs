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
    public class Type3TagController:FCardController
    {
        private const int BLOCK_COUNT = 0x0d;
        private const ushort SYSTEM_CODE = 0x12fc;
        private const ushort TYPE3_SERVICE = 0x000b;
        private const int AVAILABLE_BLOCK = 0x0d;
        private byte[] data;

        public Type3TagController(FakeFelica fFelica, byte[] idm, NdefMessage ndef)
            :base(fFelica, idm)
        {
            data = GetBlockData(ndef);
            systemCode = new ushort[] { SYSTEM_CODE };
        }

        protected override void OnRead(object sender, FelicaEventArgs e)
        {
            FReadEventArgs read = e as FReadEventArgs;
            for (int i = 0; i < read.Block.Count; i++)
            {
                BlockElement elm = read.Block[i];
                if (elm.ServiceCode != TYPE3_SERVICE)
                {
                    byte st1 = (byte)((0x01 << i)%0xff);
                    byte st2 = 0xa8;
                    OnError(fFelica, FCmd.ReadWERes, st1, st2);
                    return;
                }
            }
           
            int blockCount = read.Block.Count;
            int index = 0;
            byte[] res = new byte[2 + 1 + blockCount * Felica.BLOCK_LENGTH];//ST[2],BLOCK,DATA[16*BLOCK]
            res[index++] = 0;//ST
            res[index++] = 0;//ST

            res[index++] = (byte)blockCount;
            for (int i = 0; i < blockCount; i++)
            {
                BlockElement elm = read.Block[i];
                if (elm.Number * Felica.BLOCK_LENGTH < data.Length)
                {
                    Array.Copy(data,
                        elm.Number * Felica.BLOCK_LENGTH,
                        res,
                        index + i * Felica.BLOCK_LENGTH,
                        Felica.BLOCK_LENGTH);
                }
            }
            fFelica.DoResponse(FCmd.ReadWERes, res);
        }

        private byte[] GetBlockData(NdefMessage ndef)
        {
            byte[] msg = ndef.ToBytes();
            byte[] info = new byte[Felica.BLOCK_LENGTH];
            //ndef version
            info[0] = 0x10;

            info[1] = 0x04;
            info[2] = 0x01;

            //available block
            EndianConverter.SetUIntToBytes((ushort)AVAILABLE_BLOCK, info, 3);

            //read only
            info[10] = 0x00;

            //data length
            byte[] len = EndianConverter.UIntergerToBytes((uint)msg.Length);
            Array.Copy(len, 1, info, 11, 3);

            //check sum
            int sum = 0;
            for (int i = 0; i < 14; i++)
            {
                sum += info[i];
            }
            EndianConverter.SetUIntToBytes((ushort)sum, info, 14);

            byte[] data = new byte[BLOCK_COUNT * Felica.BLOCK_LENGTH];

            int index = 0;
            info.CopyTo(data, 0);
            index += info.Length;
            msg.CopyTo(data, index);
            return data;
        }
    }
}
