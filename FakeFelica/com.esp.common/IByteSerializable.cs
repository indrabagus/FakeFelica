using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.esp.common
{
    interface IByteSerializable
    {
        byte[] ToBytes();
    }
}
