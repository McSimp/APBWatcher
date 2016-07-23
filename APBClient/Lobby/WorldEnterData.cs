using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APBClient.Lobby
{
    public class WorldEnterData
    {
        public int ReturnCode;
        public IPAddress WorldServerIpAddress;
        public ushort WorldServerPort;
        public ulong Timestamp;
        public IPAddress PingServerIpAddress;
    }
}
