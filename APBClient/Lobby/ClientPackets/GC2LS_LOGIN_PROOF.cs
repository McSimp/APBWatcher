﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBClient.Networking;

namespace APBClient.Lobby
{
    public partial class LobbyClient
    {
        private class GC2LS_LOGIN_PROOF : ClientPacket
        {
            public GC2LS_LOGIN_PROOF(byte[] clientPub, byte[] proof)
            {
                OpCode = (uint)APBOpCode.GC2LS_LOGIN_PROOF;

                AllocateData(94);
                Writer.Write(clientPub);
                // clientPub has 64 bytes allocated for it, so write zeros to the rest
                for (int i = clientPub.Length; i < 64; i++)
                {
                    Writer.Write(0);
                }
                Writer.Write((ushort) clientPub.Length);
                Writer.Write(proof);
                // proof has 20 bytes allocated for it, so write zeros to the rest
                for (int i = proof.Length; i < 20; i++)
                {
                    Writer.Write(0);
                }
            }
        }
    }
}
