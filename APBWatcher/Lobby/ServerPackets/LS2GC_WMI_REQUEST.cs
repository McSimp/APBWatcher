using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using APBWatcher.Crypto;
using APBWatcher.Networking;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient
    {
        [PacketHandler(LobbyOpCode.LS2GC_WMI_REQUEST)]
        private class LS2GC_WMI_REQUEST : BasePacketHandler<LobbyClient>
        {
            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                // Read data from packet
                uint hwVValue = reader.ReadUInt32();
                int encryptedDataSize = reader.ReadInt32();
                byte[] encryptedData = reader.ReadBytes(encryptedDataSize);

                // Decrypt data
                byte[] decryptedData = WindowsRSA.DecryptData(client._clientDecryptEngine, encryptedData);

                // Create reader for decrypted data
                var dataReader = new APBBinaryReader(new MemoryStream(decryptedData));

                string queryLanguage = dataReader.ReadASCIIString(4);
                if (queryLanguage != "WQL")
                {
                    Log.Warn($"Unexpected query language for WMI request ({queryLanguage})");
                    client.Disconnect();
                    return;
                }

                int numSections = dataReader.ReadInt32();
                int numFields = dataReader.ReadInt32();

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;

                // Create array to store hashes of sections
                var hashes = new List<byte[]>(numSections);

                // Create XML writer to write response for WMI queries
                StringBuilder hwBuilder = new StringBuilder();
                XmlWriter hwWriter = XmlWriter.Create(hwBuilder, settings);

                hwWriter.WriteStartElement("HW");
                hwWriter.WriteAttributeString("v", hwVValue.ToString());

                // Read each section, which contains data on a WQL query for the HW part of the response
                for (int i = 0; i < numSections; i++)
                {
                    byte sectionNumber = dataReader.ReadByte();
                    byte sectionNameLength = dataReader.ReadByte();
                    string sectionName = dataReader.ReadASCIIString(sectionNameLength + 1);
                    byte skipHash = dataReader.ReadByte();
                    byte selectLength = dataReader.ReadByte();
                    string selectClause = dataReader.ReadASCIIString(selectLength + 1);
                    byte fromLength = dataReader.ReadByte();
                    string fromClause = dataReader.ReadASCIIString(fromLength + 1);

                    Log.Info($"WMI Query: Section={sectionName}, SkipHash={skipHash}, Query=SELECT {selectClause} {fromClause}");

                    byte[] hash = client._hardwareStore.BuildWmiSectionAndHash(hwWriter, sectionName, selectClause, fromClause, (skipHash == 1));
                    if (hash != null)
                    {
                        hashes.Add(hash);
                    }
                }

                hwWriter.WriteEndElement();
                hwWriter.Flush();

                // Create the middle section, which is the first 4 bytes of each hash concatenated together
                byte[] hashBlock = new byte[4 * hashes.Count];
                for (int i = 0; i < hashes.Count; i++)
                {
                    Buffer.BlockCopy(hashes[i], 0, hashBlock, i * 4, 4);
                }

                // Now we need to prepare the BFP section, which in APB is done with similar code to that at https://github.com/cavaliercoder/sysinv/
                StringBuilder bfpBuilder = new StringBuilder();
                XmlWriter bfpWriter = XmlWriter.Create(bfpBuilder, settings);
                client._hardwareStore.BuildBfpSection(bfpWriter);
                bfpWriter.Flush();

                // Generate the hash for the BFP section
                byte[] bfpHash = client._hardwareStore.BuildBfpHash();

                // Generate the Windows information section
                byte[] windowsInfo = client._hardwareStore.BuildWindowsInfo();

                // Encrypt the BFP and HW sections with our public key
                byte[] hwUnicodeData = Encoding.Unicode.GetBytes(hwBuilder.ToString());
                byte[] bfpUnicodeData = Encoding.Unicode.GetBytes(bfpBuilder.ToString());

                byte[] encryptedHWData = WindowsRSA.EncryptData(client._serverEncryptEngine, hwUnicodeData);
                byte[] encryptedBFPData = WindowsRSA.EncryptData(client._serverEncryptEngine, bfpUnicodeData);

                // Construct and send the response!
                var hardwareInfo = new GC2LS_HARDWARE_INFO(windowsInfo, 0, 0, client._hardwareStore.BfpVersion, bfpHash, hashBlock, encryptedBFPData, encryptedHWData);
                client.SendPacket(hardwareInfo);
            }
        }
    }
}
