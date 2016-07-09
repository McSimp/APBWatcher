using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Org.BouncyCastle.Crypto.Encodings;
using APBWatcher.Networking;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient : APBClient
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        byte[] m_srpKey;
        Pkcs1Encoding m_clientDecryptEngine;
        Pkcs1Encoding m_serverEncryptEngine;
        HardwareStore m_hardwareStore;

        public event EventHandler<ErrorData> OnError = delegate { };
        public event EventHandler<int> OnPuzzleFailed = delegate { };
        public event EventHandler<LoginFailedData> OnLoginFailed = delegate { };
        public event EventHandler OnLoginSuccess = delegate { };
        public event EventHandler<List<CharacterInfo>> OnCharacterList = delegate { };
        public event EventHandler<KickData> OnKick = delegate { };
        public event EventHandler<int> OnGetWorldListFailed = delegate { };
        public event EventHandler<List<WorldInfo>> OnGetWorldListSuccess = delegate { };
        public event EventHandler<int> OnWorldEnterFailed = delegate { };
        public event EventHandler OnWorldEnterSuccess = delegate { };

        string m_username;
        string m_password;

        public LobbyClient(string username, string password)
        {
            m_username = username;
            m_password = password;
            m_hardwareStore = new HardwareStore("hw.yml");
        }

        public void WorldEnter(int characterSlotNumber)
        {
            var request = new GC2LS_ASK_WORLD_ENTER(characterSlotNumber);
            SendPacket(request);
        }

        public void GetWorldList()
        {
            var worldListReq = new GC2LS_ASK_WORLD_LIST();
            SendPacket(worldListReq);
        }

        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '.' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }
    }
}

/*
 * DONE eventOnKick (  int nReason, struct FString sInformation  )
 * DONE eventOnError(  int nMessageId, int nQueryId, int nReturnCode, int nParam1, int nParam2, int nParam3, int nParam4  )
 * DONE eventOnDisconnect
 * DONE eventOnWorldEnterFailed ( int nError )
 * DONE eventOnWorldEnterSuccess ( )
 * DONT CARE eventOnCharacterInfoFailed ( int nError )
 * DONT CARE eventOnCharacterInfoSuccess ( int nSlotNumber )
 * DONT CARE eventOnWorldStatus ( int nWorldUID, int nStatus )
 * DONE eventOnGetWorldListFailed ( int nError )
 * DONE eventOnGetWorldListSuccess ( )
 * DONE eventOnCharacterList ( )
 * DONE eventOnLoginFailed ( int nError, struct FString sCountryCode )
 * DONE eventOnLoginSuccess ( )
 * DONE eventOnPuzzleFailed ( int nError )
 * DONE eventOnConnectFailed ( )
 * DONE eventOnConnectSuccess ( )
 * DONT CARE eventConnectToLS ( )
 * DONT CARE eventOnSaveConfigFailed ( int nError, int nIndex )
 * DONT CARE eventOnSaveConfigSuccess ( int nIndex )
 * DONT CARE eventOnLoadConfigFailed ( int nError, int nIndex )
 * DONT CARE eventOnLoadConfigSuccess ( int nIndex )
 * DONT CARE eventOnCharacterDeleteFailed ( int nError )
 * DONT CARE eventOnCharacterDeleteSuccess ( )
 * DONT CARE eventOnCharacterCreateFailed ( int nError )
 * DONT CARE eventOnCharacterCreateSuccess ( int nSlotNumber )
 * DONT CARE eventOnNameChangeFailed ( int nError )
 * DONT CARE eventOnNameChangeSuccess ( int nSlotNumber )
 * DONT CARE eventOnNameCheckFailed ( int nError )
 * DONT CARE eventOnNameCheckSuccess ( )
 * DONT CARE eventCharacterGetNumAdditionalSlots ( )
*/