using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace APBWatcher
{
    class HardwareStore
    {
        class WMISection
        {
            public string Select { get; set; }
            public string From { get; set; }
            public List<string> NumericFields { get; set; }
            public List<Dictionary<string, string>> Data { get; set; }

            public WMISection()
            {
                NumericFields = new List<string>();
                Data = new List<Dictionary<string, string>>();
            }
        }

        class WindowsVersionInfo
        {
            public int MajorVersion { get; set; }
            public int MinorVersion { get; set; }
            public int ProductType { get; set; }
            public int BuildNumber { get; set; }
        }

        class HardwareDB
        {
            public Dictionary<string, WMISection> WMISections { get; set; }
            public string SMBIOSVersion { get; set; }
            public int BFPVersion { get; set; }
            public Dictionary<string, Dictionary<string, string>> BFPSections { get; set; }
            public string HDDGuid { get; set; }
            public WindowsVersionInfo WindowsVersion { get; set; }
            public uint InstallDate { get; set; }
        }

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private HardwareDB m_hardwareDB;

        public int BFPVersion
        {
            get
            {
                return m_hardwareDB.BFPVersion;
            }
        }

        public HardwareStore(string storeFile)
        {
            using (TextReader reader = File.OpenText(storeFile))
            {
                var deserializer = new Deserializer();
                m_hardwareDB = deserializer.Deserialize<HardwareDB>(reader);
            }
        }

        private WMISection GetSection(string sectionName)
        {
            // Ensure we have data on the section
            if (!m_hardwareDB.WMISections.ContainsKey(sectionName))
            {
                throw new Exception(String.Format("No WMI data present for {0}", sectionName));
            }

            return m_hardwareDB.WMISections[sectionName];
        }

        public byte[] BuildWindowsInfo()
        {
            byte[] data = new byte[33];
            Buffer.BlockCopy(BitConverter.GetBytes(m_hardwareDB.WindowsVersion.MajorVersion), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(m_hardwareDB.WindowsVersion.MinorVersion), 0, data, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(m_hardwareDB.WindowsVersion.ProductType), 0, data, 8, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(m_hardwareDB.WindowsVersion.BuildNumber), 0, data, 9, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(m_hardwareDB.InstallDate), 0, data, 13, 4);
            Buffer.BlockCopy(Guid.Parse(m_hardwareDB.HDDGuid).ToByteArray(), 0, data, 17, 16);

            return data;
        }

        public void BuildBFPSection(XmlWriter writer)
        {
            writer.WriteStartElement("BFP");
            writer.WriteAttributeString("bfp_v", m_hardwareDB.BFPVersion.ToString());
            writer.WriteAttributeString("smb_v", m_hardwareDB.SMBIOSVersion);

            foreach (var section in m_hardwareDB.BFPSections)
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
                        writer.WriteElementString("RomSize", String.Format("Bios Rom Size: {0} ({1}K) == 64K * ({2}+1)", entryData, (Int32.Parse(entryData)+1)*64, entryData));
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

        public byte[] BuildBFPHash()
        {
            MemoryStream memStream = new MemoryStream(512);
            BinaryWriter writer = new BinaryWriter(memStream);
            var bfp = m_hardwareDB.BFPSections;

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
                            writer.Write(Byte.Parse(data));
                        }
                        else if (sectionName == "SYSINFO" && name == "UUID")
                        {
                            writer.Write(Guid.Parse(data).ToByteArray());
                        }
                        else if ((sectionName == "CHASSIS" || sectionName == "PROCESSOR") && name == "Type")
                        {
                            writer.Write(Byte.Parse(data));
                        }
                        else if (sectionName == "PROCESSOR" && name == "Family")
                        {
                            writer.Write(Byte.Parse(data));
                        }
                        else if (sectionName == "PROCESSOR" && name == "RawId")
                        {
                            // Skip
                        }
                        else if (sectionName == "MEMSLOTS" && name == "MaxCapacity")
                        {
                            writer.Write(UInt64.Parse(data));
                        }
                        else if (sectionName == "MEMSLOTS" && name == "NumMemoryDevices")
                        {
                            writer.Write(UInt16.Parse(data));
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

        public byte[] BuildWMISectionAndHash(XmlWriter writer, string sectionName, string select, string from, bool skipHash)
        {
            WMISection section = GetSection(sectionName);
            
            // Check if the SELECT and FROM clauses are the same in our static data and the requested data
            // It's not the end of the world if they're different, but we might send some dodgy data
            if (section.Select != select || section.From != from)
            {
                Log.Warn(String.Format("Queries do not match for '{0}' section: request=SELECT {1}, saved=SELECT {2}", sectionName, section.Select + section.From, select + from));
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
                        Log.Warn(String.Format("Missing field '{0}' from a data entry in the '{1}' section", fieldName, sectionName));
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
                            numericValues.Add(Int32.Parse(fieldValue));   
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
