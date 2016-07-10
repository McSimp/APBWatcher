using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBWatcher.Networking
{
    public class ErrorData
    {
        public uint MessageId;
        public ushort QueryId;
        public uint ReturnCode;
        public uint Param1;
        public uint Param2;
        public uint Param3;
        public uint Param4;
    }
}
