using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Org.BouncyCastle.Crypto.Encodings;
using APBWatcher.Networking;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient : BaseClient
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private byte[] _srpKey;
        private Pkcs1Encoding _clientDecryptEngine;
        private Pkcs1Encoding _serverEncryptEngine;
        private HardwareStore _hardwareStore;
        private string _username;
        private string _password;

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

        public LobbyClient(string username, string password, HardwareStore hw)
        {
            _username = username;
            _password = password;
            _hardwareStore = hw;
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