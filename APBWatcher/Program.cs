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

        static void Main(string[] args)
        {
            Dictionary<string, string> config;
            using (var configReader = File.OpenText("watcher_conf.yml"))
            {
                var deserializer = new Deserializer();
                config = deserializer.Deserialize<Dictionary<string, string>>(configReader);
            }
            
            var influxClient = new InfluxDb(config["influx_host"], config["influx_username"], config["influx_password"], InfluxVersion.Auto);

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
                        var client = new APBClient.APBClient(config["apb_username"], config["apb_password"], hw, sf);
                        await client.Login();
                        Console.WriteLine("Logged In!");
                        List<CharacterInfo> characters = client.GetCharacters();
                        Console.WriteLine("Got characters!");
                        List<WorldInfo> worlds = await client.GetWorlds();
                        Console.WriteLine("Received worlds!");
                        CharacterInfo chosenCharacter = characters[0];
                        FinalWorldEnterData worldEnterData = await client.EnterWorld(chosenCharacter.SlotNumber);
                        Console.WriteLine("Connected to world!");
                        Dictionary<int, DistrictInfo> districts = client.GetDistricts();
                        Console.WriteLine("Got districts");
                        int i = 0;
                        while (true)
                        {
                            List<InstanceInfo> instances = await client.GetInstances();
                            Console.WriteLine("Recieved instances");
                            if (i % 2 == 0)
                            {
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

                                    Console.WriteLine(String.Format("DistrictUID={0}, SDD={1:X}, Instance={2}, Threat={3}, Crims={4}, Enfs={5}, Status={6} ({7})", instance.DistrictUid, districts[instance.DistrictUid].DistrictInstanceTypeSdd, instance.InstanceNum, instance.Threat, instance.Criminals, instance.Enforcers, instance.DistrictStatus, name));

                                    var point = BuildPoint(instance, districts[instance.DistrictUid], chosenCharacter);
                                    var resp = await influxClient.WriteAsync("apb", point);
                                }
                            }

                            System.Threading.Thread.Sleep(60000);
                            i++;
                        }
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
