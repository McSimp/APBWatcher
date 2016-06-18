using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBWatcher.Lobby
{
    class GC2LS_KEY_EXCHANGE : ClientPacket
    {
        public GC2LS_KEY_EXCHANGE(byte[] encryptedKey)
        {
            OpCode = 1016;

            AllocateData(264);
            Writer.Write(0);
            Writer.Write(encryptedKey);
        }
    }
}
