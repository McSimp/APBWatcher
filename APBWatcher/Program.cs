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
            kCLIENT_STATE_WORLD_LIST_RECEIVED = 6
        };

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static ClientState state = ClientState.kCLIENT_STATE_DISCONNECTED;

        static void Main(string[] args)
        {
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
            state = ClientState.kCLIENT_STATE_LOGINSERVER_CONNECT_IN_PROGRESS;
            lc.ConnectProxy(host, port, "127.0.0.1", 9050, null, null);
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

        static void SRPTest()
        {

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
            allDone.Set();
        }
    }
}
