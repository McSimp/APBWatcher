using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBWatcher.World
{
    // Threat 4 == gold, 1 == green, 2 == bronze, 3 == silver, 0 == no threat
    public class InstanceInfo
    {
        public int DistrictUid;
        public int InstanceNum;
        public short Enforcers;
        public short Criminals;
        public byte DistrictStatus;
        public short QueueSize;
        public int Threat;
    }
}
