using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using com.esp.common;

namespace nfc.ndef
{
    public enum TNF
    {
        Empty=0,
        NfcWkt=1,
        Mime=2,
        Uri=3,
        NfcExt=4,
        Unknown=5,
        Unchanged=6,
        Reserved=7,
    }


    public class NdefMessage
    {
        public List<Record> Record { get; private set; }

        public NdefMessage()
        {
            Record = new List<Record>();
        }

        public byte[] ToBytes()
        {
            if(Record.Count == 9)
                return null;

            Record[0].Header.Mb = true;
            Record[Record.Count - 1].Header.Me = true;

            using(MemoryStream ms = new MemoryStream())
            {
                foreach(IByteSerializable record in Record)
                {
                    byte[] data = record.ToBytes();
                    ms.Write(data, 0, data.Length);
                }
                byte[] ret= ms.ToArray();
                ms.Close();
                return ret;
            }
        }
    }

    public class NdefHeader
    {
        public bool Mb { get; set; }
        public bool Me { get; set; }
        public bool Cf { get; set; }
        public bool Sr { get; set; }
        public bool Il { get; set; }
        public TNF Tnf { get; set; }

        public byte ToByte()
        {
            byte b =
                  (byte)((Mb ? 0x80 : 0)
                 | (Me ? 0x40 : 0)
                 | (Cf ? 0x20 : 0)
                 | (Sr ? 0x10 : 0)
                 | (Il ? 0x08 : 0));
            b |= (byte)Tnf;
            return b;
        }
    }

    public abstract class Record:IByteSerializable
    {
        public NdefHeader Header { get; set; }
        public virtual byte[] ToBytes()
        {
            return null;
        }
    }

    public class ShortRecord : Record
    {
        public byte[] Id { get; set; }
        public byte[] Payload { get; set; }
        public byte[] RecordType { get; set; }

        public ShortRecord()
        {
            Header = new NdefHeader() { Sr = true };
        }

        public override byte[] ToBytes()
        {
            byte[] buffer = new byte[
                                1 +//Header
                                1 +//TypeLen
                                1 +//PayloadLen
                                (Header.Il ? 1 : 0) +//IdLen
                                (Header.Tnf != TNF.Empty ? RecordType.Length : 0) +//RecordType
                                (Header.Il ? Id.Length : 0) +//Id
                                (Payload != null ? Payload.Length : 0)];//Payload
            //Header
            int index = 0;
            buffer[index++] = Header.ToByte();
            buffer[index++] = (byte)(Header.Tnf != TNF.Empty ?
                                RecordType.Length : 0);
            buffer[index++] = (byte)(Payload != null ?
                                    Payload.Length : 0);

            if(Header.Il)
                buffer[index++] = (byte)Id.Length;
            
            if (Header.Tnf != TNF.Empty)
            {
                Array.Copy(RecordType, 0, buffer, index, RecordType.Length);
                index += RecordType.Length;
            }

            if (Header.Il)
            {
                Array.Copy(Id, 0, buffer, index, Id.Length);
                index += Id.Length;
            }

            if (Payload != null)
            {
                Array.Copy(Payload, 0, buffer, index, Payload.Length);
                index += Payload.Length;
            }

            return buffer;
        }
    }
}
