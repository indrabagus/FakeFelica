using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace com.esp.common
{
    /// <summary>
    /// Change Endian
    /// </summary>
    public class EndianConverter
    {
        private EndianConverter()
        {
        }

        /// <summary>
        /// Unsigned Integer to Bigenditan array
        /// </summary>
        /// <param name="integer">ulong, uint, ushort, byte</param>
        /// <returns>byte[]</returns>
        public static byte[] UIntergerToBytes(Object integer)
        {
            if (integer.GetType() == typeof(UInt64))
            {
                byte[] buf = new byte[sizeof(UInt64)];
                UInt64 i = (UInt64)integer;
                buf[0]= (byte)(i>>56);
                buf[1]= (byte)(i>>48);
                buf[2]= (byte)(i>>40);
                buf[3]= (byte)(i>>32);
                buf[4]= (byte)(i>>24);
                buf[5]= (byte)(i>>16);
                buf[6]= (byte)(i>>8);
                buf[7]= (byte)(i);
                return buf;
            }
            else if (integer.GetType() == typeof(UInt32))
            {
                byte[] buf = new byte[sizeof(UInt32)];
                UInt32 i = (UInt32)integer;
                buf[0] = (byte)(i >> 24);
                buf[1] = (byte)(i >> 16);
                buf[2] = (byte)(i >> 8);
                buf[3] = (byte)(i);
                return buf;
            }
            else if (integer.GetType() == typeof(UInt16))
            {
                byte[] buf = new byte[sizeof(UInt16)];
                UInt16 i = (UInt16)integer;
                buf[0] = (byte)(i >> 8);
                buf[1] = (byte)(i);
                return buf;
            }
            else if (integer.GetType() == typeof(Byte))
            {
                return new byte[] { (byte)integer };
            }
            return null;
        }


        /// <summary>
        /// Set Integer at cpecify state
        /// </summary>
        /// <param name="integer">ulong, uint, ushort, byte</param>
        /// <param name="buffer">buffer</param>
        /// <param name="offset">offset</param>
        public static void SetUIntToBytes(Object integer, byte[] buf, int offset)
        {
            if (integer.GetType() == typeof(UInt64))
            {
                UInt64 i = (UInt64)integer;
                buf[offset++] = (byte)(i >> 56);
                buf[offset++] = (byte)(i >> 48);
                buf[offset++] = (byte)(i >> 40);
                buf[offset++] = (byte)(i >> 32);
                buf[offset++] = (byte)(i >> 24);
                buf[offset++] = (byte)(i >> 16);
                buf[offset++] = (byte)(i >> 8);
                buf[offset++] = (byte)(i);
            }
            else if (integer.GetType() == typeof(UInt32))
            {
                UInt32 i = (UInt32)integer;
                buf[offset++] = (byte)(i >> 24);
                buf[offset++] = (byte)(i >> 16);
                buf[offset++] = (byte)(i >> 8);
                buf[offset++] = (byte)(i);
            }
            else if (integer.GetType() == typeof(UInt16))
            {
                UInt16 i = (UInt16)integer;
                buf[offset++] = (byte)(i >> 8);
                buf[offset++] = (byte)(i);
            }
            else if (integer.GetType() == typeof(Byte))
            {
                buf[offset++] = (byte)integer;
            }
        }

        /// <summary>
        /// Get Integer from big endian array 
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="offset">offset</param>
        /// <param name="length">length</param>
        /// <returns>ulong, uint, ushort, byte</returns>
        public static object BytesToUInteger(byte[] buffer, int offset, int length)
        {
            if (length == 8)
            {
                UInt64 i = (
                       (UInt64)buffer[offset] << 56
                     | (UInt64)buffer[offset+1] << 48
                     | (UInt64)buffer[offset+2] << 40
                     | (UInt64)buffer[offset+3] << 32
                     | (UInt64)buffer[offset+4] << 24
                     | (UInt64)buffer[offset+5] << 16
                     | (UInt64)buffer[offset+6] << 8
                     | (UInt64)buffer[offset+7]);
                return i;
            }
            else if (length == 4)
            {
                UInt32 i = (
                       (UInt32)buffer[offset] << 24
                     | (UInt32)buffer[offset+1] << 16
                     | (UInt32)buffer[offset+2] << 8
                     | (UInt32)buffer[offset+3]);
                return i;
            }
            else if (length == 2)
            {
                UInt32 i = (
                       (UInt32)buffer[offset] << 8
                     | (UInt32)buffer[offset+1]);
                return (UInt16)i;
            }
            else if (length == 1)
            {
                return buffer[offset];
            }
            return 0;
        }

        /// <summary>
        /// Change Endian
        /// </summary>
        /// <param name="uinteger">ulong,uint,ushort</param>
        /// <returns>ulong,uint,ushort</returns>
        public static object SwapEndian(object uinteger)
        {
            if (uinteger.GetType() == typeof(UInt16))
            {
                UInt16 i = (UInt16)uinteger;
                UInt32 res = (byte)(
                      (i & 0x00ff) << 8
                    | (i & 0xff00) >> 8);
                return (ushort)res;
            }
            else if (uinteger.GetType() == typeof(UInt32))
            {
                UInt32 i = (UInt32)uinteger;
                UInt32 res = (byte)(
                      (i & 0x000000ff) << 24
                    | (i & 0x0000ff00) << 8
                    | (i & 0x00ff0000) >> 8
                    | (i & 0xff000000) >> 24);
                return res;
            }
            else if (uinteger.GetType() == typeof(UInt64))
            {
                UInt64 i = (UInt64)uinteger;
                UInt32 res = (byte)(
                      (i & 0x00000000000000ff) << 56
                    | (i & 0x000000000000ff00) << 40
                    | (i & 0x0000000000ff0000) << 24
                    | (i & 0x00000000ff000000) << 8
                    | (i & 0x000000ff00000000) >> 8
                    | (i & 0x0000ff0000000000) >> 24
                    | (i & 0x00ff000000000000) >> 40
                    | (i & 0xff00000000000000) >> 56);
                return res;
            }
            return 0;
        }
    }
}
