using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APBClient.World
{
    public class DistrictEnterInfo
    {
        public int ReturnCode;
        public IPAddress DistrictServerIpAddress;
        public ushort DistrictServerPort;
        public ulong Timestamp;
        public byte[] HandshakeHash;
        public byte[] XXTEAKey;
    }
}
