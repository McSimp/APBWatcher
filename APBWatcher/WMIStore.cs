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
    class WMIStore
    {
        public class WMISection
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

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, WMISection> m_wmiData;

        public WMIStore(string storeFile)
        {
            using (TextReader reader = File.OpenText(storeFile))
            {
                var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention());
                m_wmiData = deserializer.Deserialize<Dictionary<string, WMISection>>(reader);
            }
        }

        private WMISection GetSection(string sectionName)
        {
            // Ensure we have data on the section
            if (!m_wmiData.ContainsKey(sectionName))
            {
                throw new Exception(String.Format("No WMI data present for {0}", sectionName));
            }

            return m_wmiData[sectionName];
        }

        public byte[] BuildSectionAndHash(XmlWriter writer, string sectionName, string select, string from, bool skipHash)
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
