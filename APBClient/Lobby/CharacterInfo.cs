using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBClient.Lobby
{
    public class CharacterInfo
    {
        public enum FactionType : byte
        {
            Enforcer = 1,
            Criminal = 2
        }

        public int SlotNumber { get; set; }
        public FactionType Faction { get; set; }
        public int WorldStatus { get; set; }
        public int WorldUID { get; set; }
        public string WorldName { get; set; }
        public string CharacterName { get; set; }
        public int Rating { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
