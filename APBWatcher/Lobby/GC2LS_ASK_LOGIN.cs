using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBWatcher.Lobby
{
    class GC2LS_ASK_LOGIN : ClientPacket
    {
        public GC2LS_ASK_LOGIN(uint puzzleSolution, string email, byte loginType)
        {
            OpCode = 1000;

            AllocateData(225);
            Writer.Write(puzzleSolution);
            Writer.Write(0);
            Writer.Write(0);
            Writer.Write(0);
            Writer.Write(0);
            Writer.Write(0);
            Writer.Write(loginType);
            Writer.WriteUnicodeString(email, 130);
            Writer.WriteUnicodeString("", 66);
        }
    }
}
