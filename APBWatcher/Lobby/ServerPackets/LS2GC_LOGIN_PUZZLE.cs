using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Networking;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient
    {
        [PacketHandler(LobbyOpCode.LS2GC_LOGIN_PUZZLE)]
        private class LS2GC_LOGIN_PUZZLE : BasePacketHandler<LobbyClient>
        {
            private void SolveLoginPuzzle(uint[] v, uint[] k, byte unknown)
            {
                // v is the 8 byte thing form the login puzzle, k is the 3 other uints + an unknown number
                // k[3] will be updated to the correct number after this is done.

                for (uint guess = 0; guess < uint.MaxValue; guess++)
                {
                    k[3] = guess;

                    uint[] vClone = (uint[])v.Clone();
                    XXTEA.Encrypt(vClone, k, 6);
                    uint solution = vClone[1];

                    if (solution >> (32 - unknown) == 0 && (solution & (0x80000000 >> unknown)) != 0)
                    {
                        return;
                    }
                }

                throw new Exception("Failed to solve login puzzle");
            }

            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                int versionHigh = reader.ReadInt32();
                int versionMiddle = reader.ReadInt32();
                int versionLow = reader.ReadInt32();
                int buildNo = reader.ReadInt32();

                Log.Info($"Server Version: {versionHigh}.{versionMiddle}.{versionLow}.{buildNo}");

                byte unknown = reader.ReadByte();
                Log.Debug($"Unknown byte: {unknown}");

                uint puzzleSolution = 0;

                if (unknown > 0)
                {
                    byte[] encryptionKey = reader.ReadBytes(8);
                    client.SetEncryptionKey(encryptionKey);

                    uint[] uintEncryptionKey = new uint[2];
                    uintEncryptionKey[0] = BitConverter.ToUInt32(encryptionKey, 0);
                    uintEncryptionKey[1] = BitConverter.ToUInt32(encryptionKey, 4);

                    uint[] puzzleData = new uint[4];
                    puzzleData[3] = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        puzzleData[i] = reader.ReadUInt32();
                    }

                    try
                    {
                        SolveLoginPuzzle(uintEncryptionKey, puzzleData, unknown);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to solve login puzzle: v[0]=0x{uintEncryptionKey[0]:X}, v[1]=0x{uintEncryptionKey[1]:X}, k[0]=0x{puzzleData[0]:X}, k[1]=0x{puzzleData[1]:X}, k[2]=0x{puzzleData[2]:X}");

                        client.OnPuzzleFailed(client, 10011);
                        client.Disconnect();
                        return;
                    }

                    puzzleSolution = puzzleData[3];
                }

                Log.Info($"Login puzzle solved: answer={puzzleSolution}");

                var askLogin = new GC2LS_ASK_LOGIN(puzzleSolution, client.m_username, 0);
                client.SendPacket(askLogin);
            }
        }
    }
}
