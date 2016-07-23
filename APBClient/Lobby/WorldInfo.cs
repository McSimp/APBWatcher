using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APBClient.Lobby
{
    public class WorldInfo
    {
        public int Uid { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public int Population { get; set; }
        public int EnfFaction { get; set; }
        public int CrimFaction { get; set; }
        public int PremiumOnly { get; set; }
        public IPAddress PingIp { get; set; }
    }
}
