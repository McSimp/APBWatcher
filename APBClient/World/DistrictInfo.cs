using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBClient.World
{
    public class DistrictInfo
    {
        public static Dictionary<uint, string> NameMap = new Dictionary<uint, string>()
        {
            { 0xEEEC5B0B, "Missions-Financial-EN" },
            { 0xD794AAC8, "Fight Club-Abington Towers-EN" },
            { 0xAA5B02AA, "Dynamic Event-Waterfront - Anarchy-EN" },
            { 0x28CA6ADE, "Missions-Waterfront-EN" },
            { 0x1A79EB9B, "Social-Breakwater Marina-EN" },
            { 0x413D5EB0, "Fight Club-Baylan Shipping Storage-EN" },
            { 0xDC402E08, "Open Conflict-Waterfront-EN" },
            { 0xDB1AF08B, "Open Conflict-Financial-EN" },
        };
        
        public int DistrictUid;
        public uint DistrictInstanceTypeSdd;
        public string Name => NameMap[DistrictInstanceTypeSdd];
    }
}
