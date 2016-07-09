using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Crypto;
using APBWatcher.Networking;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient
    {
        [PacketHandler(LobbyOpCode.LS2GC_ANS_LOGIN_SUCCESS)]
        private class LS2GC_ANS_LOGIN_SUCCESS : BasePacketHandler<LobbyClient>
        {
            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                string realTag = reader.ReadUnicodeString(50);
                uint accountPremium = reader.ReadUInt32();
                ulong timeStamp = reader.ReadUInt64();
                ulong accountPermissions = reader.ReadUInt64();

                Log.Debug($"m_szRealTag = {realTag}");
                Log.Debug($"m_nAccountPremium = {accountPremium}");
                Log.Debug($"m_nTimestamp = {timeStamp}");
                Log.Debug($"m_nAccountPermissions = {accountPermissions}");

                for (int i = 0; i < 5; i++)
                {
                    Log.Debug($"m_nConfigFileVersion[{i}] = {reader.ReadInt32()}");
                }

                ushort voicePortMin = reader.ReadUInt16();
                ushort voicePortMax = reader.ReadUInt16();
                uint voiceAccountId = reader.ReadUInt32();
                string voiceUsername = reader.ReadASCIIString(17);
                string voiceKey = reader.ReadASCIIString(17);

                Log.Debug($"m_nVoicePortMin = {voicePortMin}");
                Log.Debug($"m_nVoicePortMax = {voicePortMax}");
                Log.Debug($"m_nVoiceAccountID = {voiceAccountId}");
                Log.Debug($"m_szVoiceUsername = {voiceUsername}");
                Log.Debug($"m_szUnknownVoiceKey = {voiceKey}");

                // Read the server's public key
                RsaKeyParameters serverPub = WindowsRSA.ReadPublicKeyBlob(reader);

                // Read the rest of the packet data
                string countryCode = reader.ReadUnicodeString();
                string voiceURL = reader.ReadASCIIString();

                Log.Debug($"m_nCountryCode = {countryCode}");
                Log.Debug($"m_szVoiceURL = {voiceURL}");

                // Create a new random RSA 1024 bit keypair for the client
                var generator = new RsaKeyPairGenerator();
                generator.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
                AsymmetricCipherKeyPair clientKeyPair = generator.GenerateKeyPair();
                RsaKeyParameters clientPub = (RsaKeyParameters)clientKeyPair.Public;

                // Create the decryption engine for later
                client._clientDecryptEngine = new Pkcs1Encoding(new RsaEngine());
                client._clientDecryptEngine.Init(false, clientKeyPair.Private);

                // Put the client public key into the Microsoft Crypto API format
                byte[] clientPubBlob = WindowsRSA.CreatePublicKeyBlob(clientPub);

                // Create encryption engine with the server's public key
                client._serverEncryptEngine = new Pkcs1Encoding(new RsaEngine());
                client._serverEncryptEngine.Init(true, serverPub);

                // Encrypt the client key blob to send to the server
                byte[] encryptedClientKey = WindowsRSA.EncryptData(client._serverEncryptEngine, clientPubBlob);

                // Use the SRP key we calculated before
                client.SetEncryptionKey(client._srpKey);

                var keyExchange = new GC2LS_KEY_EXCHANGE(encryptedClientKey);
                client.SendPacket(keyExchange);

                client.OnLoginSuccess(client, null);
            }
        }
    }
}
