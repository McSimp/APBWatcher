using APBClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using APBClient.Lobby;
using APBClient.Networking;
using APBClient.World;
using InfluxDB.Net;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Models;
using YamlDotNet.Serialization;

namespace APBWatcher
{
    class Program
    {
        static Point BuildPoint(InstanceInfo instance, DistrictInfo district, CharacterInfo character)
        {
            var point = new Point();

            point.Measurement = "instance_data";
            point.Fields = new Dictionary<string, object>
            {
                { "enforcers", instance.Enforcers },
                { "criminals", instance.Criminals },
                { "queue_size", instance.QueueSize },
                { "district_status", instance.DistrictStatus },
            };
            point.Tags = new Dictionary<string, object>
            {
                { "threat", instance.Threat },
                { "instance_num", instance.InstanceNum },
                { "district_uid", instance.DistrictUid },
                { "world_uid", character.WorldUID },
                { "district_instance_type_sdd", district.DistrictInstanceTypeSdd },
            };

            return point;
        }

        static async Task ScrapeForAccountCharacter(int accountIndex, int characterIndex, InfluxDb influxClient, WatcherConfig config, HardwareStore hw, ISocketFactory sf)
        {
            var client = new APBClient.APBClient(config.ApbAccounts[accountIndex]["username"], config.ApbAccounts[accountIndex]["password"], hw, sf);
            await client.Login();
            Console.WriteLine("Logged In!");
            List<CharacterInfo> characters = client.GetCharacters();
            Console.WriteLine("Got characters!");
            List<WorldInfo> worlds = await client.GetWorlds();
            Console.WriteLine("Received worlds!");
            CharacterInfo chosenCharacter = characters[characterIndex];
            FinalWorldEnterData worldEnterData = await client.EnterWorld(chosenCharacter.SlotNumber);
            Console.WriteLine("Connected to world!");
            Dictionary<int, DistrictInfo> districts = client.GetDistricts();
            Console.WriteLine("Got districts");
            List<InstanceInfo> instances = await client.GetInstances();
            Console.WriteLine("Recieved instances");
            foreach (var instance in instances)
            {
                string name = "UNKNOWN";
                try
                {
                    name = districts[instance.DistrictUid].Name;
                }
                catch (Exception e)
                {

                }

                Console.WriteLine(String.Format("DistrictUID={0}, SDD={1:X}, Instance={2}, Threat={3}, Crims={4}, Enfs={5}, Status={6}, World={7} ({8})", instance.DistrictUid, districts[instance.DistrictUid].DistrictInstanceTypeSdd, instance.InstanceNum, instance.Threat, instance.Criminals, instance.Enforcers, instance.DistrictStatus, chosenCharacter.WorldUID, name));

                var point = BuildPoint(instance, districts[instance.DistrictUid], chosenCharacter);
                var resp = await influxClient.WriteAsync("apb", point);
            }

            client.Disconnect();
        }

        static void Main(string[] args)
        {
            WatcherConfig config;
            using (var configReader = File.OpenText("watcher_conf.yml"))
            {
                var deserializer = new Deserializer();
                config = deserializer.Deserialize<WatcherConfig>(configReader);
            }
            
            var influxClient = new InfluxDb(config.InfluxHost, config.InfluxUsername, config.InfluxPassword, InfluxVersion.Auto);

            HardwareStore hw;
            using (TextReader reader = File.OpenText("hw.yml"))
            {
                hw = new HardwareStore(reader);
            }

            ISocketFactory sf = new ProxySocketFactory("127.0.0.1", 9150, null, null);

            while (true)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await ScrapeForAccountCharacter(0, 0, influxClient, config, hw, sf);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error occurred");
                        Console.WriteLine(e);
                    }

                    try
                    {
                        await ScrapeForAccountCharacter(0, 1, influxClient, config, hw, sf);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error occurred");
                        Console.WriteLine(e);
                    }

                    try
                    {
                        await ScrapeForAccountCharacter(1, 0, influxClient, config, hw, sf);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error occurred");
                        Console.WriteLine(e);
                    }
                }).Wait();

                System.Threading.Thread.Sleep(120000);
            }
        }
    }
}
