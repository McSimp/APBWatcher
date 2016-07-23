using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBClient.Networking;

namespace APBClient.Lobby
{
    public partial class LobbyClient
    {
        [PacketHandler(APBOpCode.LS2GC_CHARACTER_LIST)]
        private class LS2GC_CHARACTER_LIST : BasePacketHandler<LobbyClient>
        {
            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                byte numCharacters = reader.ReadByte();
                int numAdditionalSlots = reader.ReadInt32();
                byte accountThreat = reader.ReadByte();

                Log.Debug($"m_nCharacters = {numCharacters}");
                Log.Debug($"m_nNumAdditionalCharacterSlots = {numAdditionalSlots}");
                Log.Debug($"m_nAccountThreat = {accountThreat}");

                var characters = new List<CharacterInfo>(numCharacters);

                for (int i = 0; i < numCharacters; i++)
                {
                    var info = new CharacterInfo
                    {
                        SlotNumber = (int) reader.ReadByte(),
                        Faction = (CharacterInfo.FactionType) reader.ReadByte(),
                        WorldStatus = (int) reader.ReadByte(),
                        WorldUID = reader.ReadInt32(),
                        WorldName = reader.ReadUnicodeString(34),
                        CharacterName = reader.ReadUnicodeString(34),
                        Rating = reader.ReadInt32(),
                        LastLogin = new DateTime(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16())
                    };
                    reader.ReadInt32(); // This is the "fraction" part of last login but I don't care about it
                    characters.Add(info);
                }

                client.OnCharacterList(client, characters);
            }
        }
    }
}
