using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Org.Mentalis.Network.ProxySocket;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using System.IO;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System.Runtime.InteropServices;
using Org.BouncyCastle.OpenSsl;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Xml;
using APBWatcher.Lobby;

namespace APBWatcher
{
    class Program
    {
        public enum ClientState
        {
            kCLIENT_STATE_DISCONNECTED = 0,
            kCLIENT_STATE_LOGINSERVER_CONNECT_IN_PROGRESS = 1,
            kCLIENT_STATE_LOGINSERVER_CONNECT_COMPLETE = 2,
            kCLIENT_STATE_LOGIN_IN_PROGRESS = 3, // IGNORED
            kCLIENT_STATE_LOGIN_SUCCESS = 4,
            kCLIENT_STATE_CHARACTER_LIST_RECEIVED = 5,
            kCLIENT_STATE_WORLD_LIST_RECEIVED = 6,
        };

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static ClientState state = ClientState.kCLIENT_STATE_DISCONNECTED;

        static void TestCrypto()
        {
            IntPtr hProv = new IntPtr();
            bool res = CryptoTest.CryptAcquireContext(ref hProv, null, null, CryptoTest.PROV_RSA_FULL, CryptoTest.CRYPT_VERIFYCONTEXT);

            /*
            // Create a new random RSA 1024 bit keypair for the client
            var generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
            AsymmetricCipherKeyPair clientKeyPair = generator.GenerateKeyPair();
            RsaKeyParameters clientPub = (RsaKeyParameters)clientKeyPair.Public;

            // Put the client public key into the Microsoft Crypto API format
            byte[] clientPubData = new byte[148];

            byte[] header = {
                0x06, 0x02, 0x00, 0x00, 0x00, 0xA4, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00 
            };
            Array.Copy(header, clientPubData, header.Length);

            byte[] exponentData = clientPub.Exponent.ToByteArrayUnsigned();
            Array.Reverse(exponentData);
            Array.Copy(exponentData, 0, clientPubData, 16, exponentData.Length);

            byte[] modulusData = clientPub.Modulus.ToByteArrayUnsigned();
            Array.Reverse(modulusData);
            Array.Copy(modulusData, 0, clientPubData, 20, modulusData.Length);

            uint size = 148;
            IntPtr hPubKey = new IntPtr();
            res = CryptoTest.CryptImportKey(hProv, clientPubData, size, IntPtr.Zero, 0, ref hPubKey);

            byte[] testData = new byte[128];
            Array.Clear(testData, 0, 128);
            uint testDataLen = (uint)117;
            res = CryptoTest.CryptEncrypt(hPubKey, IntPtr.Zero, 1, 0, testData, ref testDataLen, 128);

            StringWriter strWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(strWriter);
            pemWriter.WriteObject(clientKeyPair.Public);
            
            pemWriter.Writer.Flush();
            string pubKey = strWriter.ToString();
            
            byte[] pubBlob = new byte[2048];
            uint pubLen = (uint)pubBlob.Length;
            res = CryptoTest.CryptExportKey(hPubKey, IntPtr.Zero, CryptoTest.PUBLICKEYBLOB, 0, pubBlob, ref pubLen);

            byte[] blob = new byte[2048];
            uint blobSize = 2048;
            CryptoTest.CryptEncodeObject(0x00010001, 19, pubBlob, blob, ref blobSize);

            string cryptoApi = Convert.ToBase64String(blob, 0, (int)blobSize);


            int err = Marshal.GetLastWin32Error();

            Array.Reverse(testData);
            var decryptEngine = new Pkcs1Encoding(new RsaEngine());
            decryptEngine.Init(false, clientKeyPair.Private);
            byte[] decryptedData = decryptEngine.ProcessBlock(testData, 0, 128);
            */
            

            
            IntPtr hServerKey = new IntPtr();
            res = CryptoTest.CryptGenKey(hProv, 1, 16384, out hServerKey);

            IntPtr hServerKeyDup = new IntPtr();
            res = CryptoTest.CryptDuplicateKey(hServerKey, IntPtr.Zero, 0, ref hServerKeyDup);

            byte[] pubBlob = new byte[2048];
            uint pubLen = (uint)pubBlob.Length;
            res = CryptoTest.CryptExportKey(hServerKey, IntPtr.Zero, CryptoTest.PUBLICKEYBLOB, 0, pubBlob, ref pubLen);

            //byte[] outData = new byte[128];
            //uint dataLen = 128;

            //const uint KP_ALGID = 7;
            //const uint KP_KEYLEN = 9;
            //res = CryptoTest.CryptGetKeyParam(hServerKeyDup, 4, outData, ref dataLen, 0);

            // KP_ALGID = CALG_RSA_KEYX
            // KP_KEYLEN = 1024
            // KP_PADDING = 0?
            // KP_MODE = 0?

            // Create a public key for the server
            byte[] modulus = new byte[128];
            Buffer.BlockCopy(pubBlob, 20, modulus, 0, 128);
            Array.Reverse(modulus);

            byte[] exponent = new byte[4];
            Buffer.BlockCopy(pubBlob, 16, exponent, 0, 4);
            Array.Reverse(exponent);
            var serverPub = new RsaKeyParameters(false, new BigInteger(1, modulus), new BigInteger(1, exponent));

            // Create a public key for the client
            //var clientPub = new RsaKeyParameters(false, new BigInteger(1, clientPubKey, 20, 128), new BigInteger(1, clientPubKey, 16, 4));

            byte[] testData = new byte[128];
            Array.Clear(testData, 0, 128);
            //uint testDataLen = (uint)117;
            //res = CryptoTest.CryptEncrypt(hServerKeyDup, IntPtr.Zero, 1, 0, testData, ref testDataLen, 128);
            

            //byte[] clientPubKey = new byte[117];
            //rnd.NextBytes(clientPubKey);

            // Create encryption engine with the server's public key
            var encryptEngine = new Pkcs1Encoding(new RsaEngine());
            encryptEngine.Init(true, serverPub);
            byte[] encData1 = encryptEngine.ProcessBlock(testData, 0, 117);
            Array.Reverse(encData1);

            uint encDataLen = (uint)encData1.Length;
            res = CryptoTest.CryptDecrypt(hServerKeyDup, IntPtr.Zero, 1, 0, encData1, ref encDataLen);

            int err = Marshal.GetLastWin32Error();
        }

        static void YamlTest()
        {
            var uhh = new HardwareStore(@"F:\dev\apb\APBWatcher\APBWatcher\bin\Debug\hw.yml");
            StringBuilder test = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            XmlWriter writer = XmlWriter.Create(test, settings);
            uhh.BuildWmiSectionAndHash(writer, "CPU", "ProcessorId,Manufacturer,Name,Description,Revision,L2CacheSize,@AddressWidth", "FROM Win32_Processor", false);
            writer.Flush();
            string res = test.ToString();
            var asdf = uhh.BuildWindowsInfo();
            StringBuilder test2 = new StringBuilder();
            XmlWriter writer2 = XmlWriter.Create(test2, settings);
            uhh.BuildBfpSection(writer2);
            writer2.Flush();
            string res2 = test2.ToString();
            byte[] bfpHash = uhh.BuildBfpHash();
        }

        static void Main(string[] args)
        {
            //SRPTest();
            //return;
            //TestCrypto();
            //return;
            //YamlTest();
            //return;

            string host = "apb.login.gamersfirst.com";
            int port = 1001;

            StreamReader file = new StreamReader("creds.txt");
            string username = file.ReadLine();
            string password = file.ReadLine();
            file.Close();

            ///*
            var lc = new LobbyClient(username, password);
            lc.OnConnectSuccess += lc_OnConnectSuccess;
            lc.OnConnectFailed += lc_OnConnectFailed;
            lc.OnDisconnect += lc_OnDisconnect;
            lc.OnLoginSuccess += lc_OnLoginSuccess;
            lc.OnCharacterList += lc_OnCharacterList;
            lc.OnGetWorldListSuccess += lc_OnGetWorldListSuccess;
            lc.OnWorldEnterSuccess += lc_OnWorldEnterSuccess;
            state = ClientState.kCLIENT_STATE_LOGINSERVER_CONNECT_IN_PROGRESS;
            lc.ConnectProxy(host, port, "127.0.0.1", 9150, null, null);
            //lc.Connect(host, port);
            //*/
            // TODO: Set a timeout for receiving the login salt
            /*

            byte[] hexData = {
                0xD3, 0x07, 0x00, 0x00, 0x94, 0xAF, 0xF5, 0x01, 0x80, 0x62, 0xC9, 0x50, 0x47, 0xF5, 0x44, 0xDE, 0xF4, 0x55, 0xBF, 0x57, 0xE4, 0x31, 0xD8, 0x6E, 0x4C, 0x79, 0xAC, 0x9F, 0xC7, 0x5C, 0x24, 0xE3, 0xD7, 0xF7, 0xA9, 0x7E, 0x43, 0xDB, 0xFF, 0xE8, 0x77, 0x83, 0xEB, 0x0A, 0xB1, 0xFA, 0x3A, 0xA4, 0x35, 0xA0, 0xBF, 0xD2, 0xD6, 0x4D, 0xD9, 0xCD, 0x0B, 0xE1, 0x96, 0xC3, 0xC9, 0xAC, 0x4A, 0x72, 0x87, 0x7A, 0xB5, 0x5D, 0x5D, 0x56, 0xF2, 0x6C, 0x40, 0x00, 0x84, 0x5B, 0x89, 0x46, 0xF2, 0x34, 0x62, 0x51, 0x26, 0xB4
                             };

            ServerPacket packet = new ServerPacket(hexData, 0, hexData.Length);
            var lc = new LobbyClient();
            lc.HandleLoginSalt(packet);
            */
            Console.ReadLine();
        }

        static void lc_OnWorldEnterSuccess(object sender, EventArgs e)
        {
            Console.WriteLine("World enter success!");
        }

        static void lc_OnGetWorldListSuccess(object sender, List<WorldInfo> e)
        {
            Console.WriteLine("Worlds received!");
            var lc = (LobbyClient)sender;
            lc.WorldEnter(0);
        }

        static void lc_OnCharacterList(object sender, List<CharacterInfo> e)
        {
            state = ClientState.kCLIENT_STATE_CHARACTER_LIST_RECEIVED;
            Console.Write("Characters received!");
            var lc = (LobbyClient)sender;
            lc.GetWorldList();
        }

        static void lc_OnLoginSuccess(object sender, EventArgs e)
        {
            state = ClientState.kCLIENT_STATE_LOGIN_SUCCESS;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        static void SRPTest()
        {
            string APBMod = "11144252439149533417835749556168991736939157778924947037200268358613863350040339017097790259154750906072491181606044774215413467851989724116331597513345603";

            var blah = Srp6StandardGroups.rfc5054_1024.N;
            var blah2 = new BigInteger(APBMod);

            byte[] APBg = {
                0x02
            };

            var data = "58000000D307000094AFF50107C687A473F97C3D50EF771C2FE39A510F6CA46D61D4B0171DE4EFA1DC9F14BA3FF14FF52482A851B6134023DA108E911A48B4267DBAFDF79963F396C20E79BF4000D13BBAB417D98FE1D7ED";
            var reader = new APBBinaryReader(new MemoryStream(StringToByteArray(data)));
            reader.ReadUInt32(); reader.ReadUInt32();

            // Initialise the SRP client
            var srpClient = new WeakSrp6Client();
            var secureRandom = new SecureRandom();
            srpClient.Init(new BigInteger(APBMod), new BigInteger(APBg), new Sha1Digest(), secureRandom);

            // Read data from packet
            uint accountId = reader.ReadUInt32();
            byte[] serverB = reader.ReadBytes(64);
            ushort serverBLen = reader.ReadUInt16();
            byte[] salt = reader.ReadBytes(10);

            // Transform values into types the SRP client expects
            BigInteger serverBInt = new BigInteger(1, serverB, 0, serverBLen);
            byte[] usernameBytes = Encoding.ASCII.GetBytes(accountId.ToString());
            byte[] passwordBytes = Encoding.ASCII.GetBytes("dicksdicks");

            // Calculate the client's public value
            BigInteger clientPub = srpClient.GenerateClientCredentials(salt, usernameBytes, passwordBytes);

            srpClient.CalculateSecret(serverBInt);

            byte[] key = srpClient.CalculateSessionKey().ToByteArrayUnsigned();

            // Calculate the proof that the client knows the secret
            BigInteger proof = srpClient.CalculateClientEvidenceMessage();

            Console.WriteLine(LobbyClient.HexDump(clientPub.ToByteArrayUnsigned()));
            Console.WriteLine(LobbyClient.HexDump(proof.ToByteArrayUnsigned()));
            Console.WriteLine(LobbyClient.HexDump(key));
        }

        static void lc_OnDisconnect(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected!");
            state = ClientState.kCLIENT_STATE_DISCONNECTED;
            allDone.Set();
        }

        static void lc_OnConnectFailed(object sender, Exception e)
        {
            Console.WriteLine(e.ToString());
            state = ClientState.kCLIENT_STATE_DISCONNECTED;
            allDone.Set();
        }

        static void lc_OnConnectSuccess(object sender, EventArgs e)
        {
            Console.WriteLine("Connected!");
            state = ClientState.kCLIENT_STATE_LOGINSERVER_CONNECT_COMPLETE;
        }
    }
}
