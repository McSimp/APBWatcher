using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Networking;

namespace APBWatcher.Lobby.ClientPackets
{
    internal class GC2LS_KEY_EXCHANGE : ClientPacket
    {
        public GC2LS_KEY_EXCHANGE(byte[] encryptedKey)
        {
            OpCode = 1016;

            AllocateData(268);
            Writer.Write(0);
            Writer.Write(encryptedKey);
        }
    }
}
