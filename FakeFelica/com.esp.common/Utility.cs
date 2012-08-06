using System;
using System.Collections.Generic;
using System.Text;

namespace com.esp.common
{
    /// <summary>
    /// Utility
    /// </summary>
    public class Utility
    {
        private Utility()
        {
        }

        /// <summary>
        /// Byte[] to Hex format string
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="offset">offset</param>
        /// <param name="length">length</param>
        /// <returns>hex format string</returns>
        public static string ByteToHex(byte[] buffer, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(buffer[offset + i].ToString("X2"));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Hex format string to Byte[]
        /// </summary>
        /// <param name="hex">hex format string</param>
        /// <returns>Byte[]</returns>
        public static byte[] HexToByte(string hex)
        {
            hex = hex.Replace(" ", "");
            
            byte[] ret = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length / 2; i++)
            {
                ret[i] = byte.Parse(hex.Substring(i * 2, 2),  System.Globalization.NumberStyles.HexNumber);
            }
            return ret;
        }
    }
}
