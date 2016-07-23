using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using YamlDotNet.Serialization;

namespace APBClient
{
    public class HardwareStore
    {
        private class WmiSection
        {
            public string Select { get; set; }
            public string From { get; set; }
            public List<string> NumericFields { get; set; }
            public List<Dictionary<string, string>> Data { get; set; }

            public WmiSection()
            {
                NumericFields = new List<string>();
                Data = new List<Dictionary<string, string>>();
            }
        }

        private class WindowsVersionInfo
        {
            public int MajorVersion { get; set; }
            public int MinorVersion { get; set; }
            public int ProductType { get; set; }
            public int BuildNumber { get; set; }
        }

        private class HardwareDb
        {
            public Dictionary<string, WmiSection> WmiSections { get; set; }
            public string SmbiosVersion { get; set; }
            public int BfpVersion { get; set; }
            public Dictionary<string, Dictionary<string, string>> BfpSections { get; set; }
            public string HddGuid { get; set; }
            public WindowsVersionInfo WindowsVersion { get; set; }
            public uint InstallDate { get; set; }
        }

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly HardwareDb _hardwareDb;

        public int BfpVersion => _hardwareDb.BfpVersion;

        public HardwareStore(TextReader reader)
        {
            var deserializer = new Deserializer();
            _hardwareDb = deserializer.Deserialize<HardwareDb>(reader);
        }

        private WmiSection GetSection(string sectionName)
        {
            // Ensure we have data on the section
            if (!_hardwareDb.WmiSections.ContainsKey(sectionName))
            {
                throw new Exception($"No WMI data present for {sectionName}");
            }

            return _hardwareDb.WmiSections[sectionName];
        }

        public byte[] BuildWindowsInfo()
        {
            var data = new byte[33];
            Buffer.BlockCopy(BitConverter.GetBytes(_hardwareDb.WindowsVersion.MajorVersion), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(_hardwareDb.WindowsVersion.MinorVersion), 0, data, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(_hardwareDb.WindowsVersion.ProductType), 0, data, 8, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(_hardwareDb.WindowsVersion.BuildNumber), 0, data, 9, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(_hardwareDb.InstallDate), 0, data, 13, 4);
            Buffer.BlockCopy(Guid.Parse(_hardwareDb.HddGuid).ToByteArray(), 0, data, 17, 16);

            return data;
        }

        public void BuildBfpSection(XmlWriter writer)
        {
            writer.WriteStartElement("BFP");
            writer.WriteAttributeString("bfp_v", _hardwareDb.BfpVersion.ToString());
            writer.WriteAttributeString("smb_v", _hardwareDb.SmbiosVersion);

            foreach (var section in _hardwareDb.BfpSections)
            {
                var sectionName = section.Key;
                var data = section.Value;

                writer.WriteStartElement(sectionName);

                if (sectionName == "SLOTS")
                {
                    writer.WriteAttributeString("Num", data.Count.ToString());
                }
                
                foreach (var entry in data)
                {
                    var entryName = entry.Key;
                    var entryData = entry.Value;

                    if (sectionName == "BIOS" && entryName == "RomSize")
                    {
                        var size = (int.Parse(entryData) + 1)*64;
                        writer.WriteElementString("RomSize", $"Bios Rom Size: {entryData} ({size}K) == 64K * ({entryData}+1)");
                    }
                    else
                    {
                        writer.WriteElementString(entry.Key, entry.Value);
                    }
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public byte[] BuildBfpHash()
        {
            var memStream = new MemoryStream(512);
            var writer = new BinaryWriter(memStream);
            var bfp = _hardwareDb.BfpSections;

            foreach (var section in bfp)
            {
                var sectionName = section.Key;
                if (sectionName == "SLOTS")
                {
                    writer.Write((ushort)section.Value.Count);
                }
                else
                {
                    foreach (var entry in section.Value)
                    {
                        var name = entry.Key;
                        var data = entry.Value;

                        if (sectionName == "BIOS" && name == "RomSize")
                        {
                            writer.Write(byte.Parse(data));
                        }
                        else if (sectionName == "SYSINFO" && name == "UUID")
                        {
                            writer.Write(Guid.Parse(data).ToByteArray());
                        }
                        else if ((sectionName == "CHASSIS" || sectionName == "PROCESSOR") && name == "Type")
                        {
                            writer.Write(byte.Parse(data));
                        }
                        else if (sectionName == "PROCESSOR" && name == "Family")
                        {
                            writer.Write(byte.Parse(data));
                        }
                        else if (sectionName == "PROCESSOR" && name == "RawId")
                        {
                            // Skip
                        }
                        else if (sectionName == "MEMSLOTS" && name == "MaxCapacity")
                        {
                            writer.Write(ulong.Parse(data));
                        }
                        else if (sectionName == "MEMSLOTS" && name == "NumMemoryDevices")
                        {
                            writer.Write(ushort.Parse(data));
                        }
                        else
                        {
                            writer.Write(Encoding.ASCII.GetBytes(data));
                        }
                    }
                }
            }

            byte[] hashData = memStream.ToArray();

            var sha1 = new Sha1Digest();
            sha1.BlockUpdate(hashData, 0, hashData.Length);
            var hash = new byte[sha1.GetDigestSize()];
            sha1.DoFinal(hash, 0);

            return hash;
        }

        public byte[] BuildWmiSectionAndHash(XmlWriter writer, string sectionName, string select, string from, bool skipHash)
        {
            WmiSection section = GetSection(sectionName);
            
            // Check if the SELECT and FROM clauses are the same in our static data and the requested data
            // It's not the end of the world if they're different, but we might send some dodgy data
            if (section.Select != select || section.From != from)
            {
                Log.Warn($"Queries do not match for '{sectionName}' section: request=SELECT {section.Select + section.From}, saved=SELECT {select + from}");
            }

            // Define storage for WMI values to be hashed
            var stringValues = new List<string>();
            var numericValues = new List<int>();

            // Get the requested fields from the query
            string[] fieldNames = select.Split(',');

            writer.WriteStartElement(sectionName);

            if (skipHash)
            {
                writer.WriteAttributeString("s", "1");
            }

            // For each row of saved data, write the appropriate fields to the xml stream
            for (int i = 0; i < section.Data.Count; i++)
            {
                var dataEntry = section.Data[i];

                foreach (var fieldName in fieldNames)
                {
                    bool isSkipField = false;
                    string actualFieldName = fieldName;
                    if (fieldName.StartsWith("@"))
                    {
                        isSkipField = true;
                        actualFieldName = fieldName.Substring(1);
                    }

                    // Skip the field if we don't have data for it
                    if (!dataEntry.ContainsKey(actualFieldName))
                    {
                        Log.Warn($"Missing field '{fieldName}' from a data entry in the '{sectionName}' section");
                        continue;
                    }

                    string fieldValue = dataEntry[actualFieldName];
                    writer.WriteStartElement(actualFieldName);
                    writer.WriteAttributeString("n", (i+1).ToString());
                    
                    // If the field starts with an @, we need to add s="1"
                    if (isSkipField || skipHash)
                    {
                        writer.WriteAttributeString("s", "1");
                    }
                    else
                    {
                        if (section.NumericFields.Contains(fieldName))
                        {
                            numericValues.Add(int.Parse(fieldValue));   
                        }
                        else
                        {
                            stringValues.Add(fieldValue);
                        }
                    }

                    writer.WriteValue(fieldValue);
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();

            // Sort the values and hash if necessary
            if (!skipHash)
            {
                numericValues.Sort();
                stringValues.Sort();

                var sha1 = new Sha1Digest();
                foreach (int value in numericValues)
                {
                    sha1.BlockUpdate(BitConverter.GetBytes(value), 0, 4);
                }

                foreach (string value in stringValues)
                {
                    byte[] rawData = Encoding.Unicode.GetBytes(value);
                    sha1.BlockUpdate(rawData, 0, rawData.Length);
                }

                byte[] hash = new byte[sha1.GetDigestSize()];
                sha1.DoFinal(hash, 0);

                return hash;
            }

            return null;
        }
    }
}
