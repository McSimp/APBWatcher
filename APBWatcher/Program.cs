using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using APBWatcher.Lobby;
using APBWatcher.World;

namespace APBWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader file = new StreamReader("creds.txt");
            string username = file.ReadLine();
            string password = file.ReadLine();
            file.Close();

            var hw = new HardwareStore("hw.yml");

            Task.Run(async () =>
            {
                try
                {
                    APBClient client = new APBClient(username, password, "hw.yml");
                    await client.Login();
                    Console.WriteLine("Logged In!");
                    List<CharacterInfo> characters = client.GetCharacters();
                    Console.WriteLine("Got characters!");
                    List<WorldInfo> worlds = await client.GetWorlds();
                    Console.WriteLine("Received worlds!");
                    FinalWorldEnterData worldEnterData = await client.EnterWorld(characters[0].SlotNumber);
                    Console.WriteLine("Connected to world!");
                    List<InstanceInfo> instances = await client.GetInstances();
                    Console.WriteLine("Recieved instances");
                    Dictionary<int, DistrictInfo> districts = client.GetDistricts();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to do stuff");
                    Console.WriteLine(e);
                }
            }).Wait();

            return;
            Console.ReadLine();
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
