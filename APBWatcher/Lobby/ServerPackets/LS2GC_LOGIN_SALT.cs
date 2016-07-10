using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Crypto;
using APBWatcher.Networking;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient
    {
        [PacketHandler(APBOpCode.LS2GC_LOGIN_SALT)]
        private class LS2GC_LOGIN_SALT : BasePacketHandler<LobbyClient>
        {
            private const string Modulus = "11144252439149533417835749556168991736939157778924947037200268358613863350040339017097790259154750906072491181606044774215413467851989724116331597513345603";
            private static readonly byte[] Generator = {
                0x02
            };

            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                // Initialise the SRP client
                var srpClient = new WeakSrp6Client();
                var secureRandom = new SecureRandom();
                srpClient.Init(new BigInteger(Modulus), new BigInteger(Generator), new Sha1Digest(), secureRandom);

                // Read data from packet
                uint accountId = reader.ReadUInt32();
                byte[] serverB = reader.ReadBytes(64);
                ushort serverBLen = reader.ReadUInt16();
                byte[] salt = reader.ReadBytes(10);

                Log.Info($"Account ID = 0x{accountId:X}");
                client._accountId = accountId;

                // Transform values into types the SRP client expects
                BigInteger serverBInt = new BigInteger(1, serverB, 0, serverBLen);
                byte[] usernameBytes = Encoding.ASCII.GetBytes(accountId.ToString());
                byte[] passwordBytes = Encoding.ASCII.GetBytes(client._password);

                // Calculate the client's public value
                BigInteger clientPub = srpClient.GenerateClientCredentials(salt, usernameBytes, passwordBytes);

                srpClient.CalculateSecret(serverBInt);
                client._srpKey = srpClient.CalculateSessionKey().ToByteArrayUnsigned();

                // Calculate the proof that the client knows the secret
                BigInteger proof = srpClient.CalculateClientEvidenceMessage();

                var loginProof = new GC2LS_LOGIN_PROOF(clientPub.ToByteArrayUnsigned(), proof.ToByteArrayUnsigned());
                client.SendPacket(loginProof);
            }
        }
    }
}
