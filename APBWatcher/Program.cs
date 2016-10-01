using APBClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBClient.Lobby;
using APBClient.Networking;
using APBClient.World;

namespace APBWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = new StreamReader("creds.txt");
            string username = file.ReadLine();
            string password = file.ReadLine();
            file.Close();

            HardwareStore hw;
            using (TextReader reader = File.OpenText("hw.yml"))
            {
                hw = new HardwareStore(reader);
            }

            ISocketFactory sf = new ProxySocketFactory("127.0.0.1", 9050, null, null);

            Task.Run(async () =>
            {
                try
                {
                    var client = new APBClient.APBClient(username, password, hw, sf);
                    await client.Login();
                    Console.WriteLine("Logged In!");
                    List<CharacterInfo> characters = client.GetCharacters();
                    Console.WriteLine("Got characters!");
                    List<WorldInfo> worlds = await client.GetWorlds();
                    Console.WriteLine("Received worlds!");
                    FinalWorldEnterData worldEnterData = await client.EnterWorld(characters[1].SlotNumber);
                    Console.WriteLine("Connected to world!");
                    Dictionary<int, DistrictInfo> districts = client.GetDistricts();
                    Console.WriteLine("Got districts");
                    List<InstanceInfo> instances = await client.GetInstances();
                    Console.WriteLine("Recieved instances");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occurred");
                    Console.WriteLine(e);
                }
            }).Wait();
        }
    }
}
