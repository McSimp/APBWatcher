using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using APBClient.Lobby;
using APBClient.Networking;
using APBClient.World;
using log4net.Util;

namespace APBClient
{
    public class APBClient
    {
        interface IGenericTCS
        {
            void SetResult(object result);
            void SetTaskException(Exception e);
            Task BaseTask { get; }
        }

        class VirtualTCS<T> : IGenericTCS
        {
            private TaskCompletionSource<T> _tcs;

            public VirtualTCS()
            {
                _tcs = new TaskCompletionSource<T>();
            }

            public void SetResult(object result)
            {
                _tcs.SetResult((T)result);
            }

            public void SetTaskException(Exception e)
            {
                Task<T> task = _tcs.Task;
                if (task?.Status == TaskStatus.WaitingForActivation)
                {
                    _tcs.SetException(e);
                }
            }

            public Task<T> Task => _tcs.Task;
            public Task BaseTask => _tcs.Task;
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class RequiredStateAttribute : Attribute
        {
            public readonly ClientState RequiredState;

            public RequiredStateAttribute(ClientState requiredState)
            {
                RequiredState = requiredState;
            }
        }

        public enum ClientState
        {
            Disconnected,
            LobbyServerConnectInProgress,
            LobbyServerConnectComplete, // IGNORED
            LobbyServerLoginInProgress, 
            LobbyServerLoginComplete,
            LobbyServerCharacterListReceived,
            LobbyServerWorldListInProgress,
            LobbyServerWorldEnterInProgress,
            LobbyServerWorldEnterComplete,
            WorldServerConnectInProgress,
            WorldServerConnectComplete,
            WorldServerWorldEnterInProgress, // IGNORED
            WorldServerWorldEnterComplete,
            WorldServerInstanceListInProgress,
            WorldServerDistrictReserveInProgress,
            WorldServerDistrictEnterInProgress,
            WorldServerDistrictEnterComplete
        }

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string LobbyHost = "apb.login.gamersfirst.com";
        private const int LobbyPort = 1001;

        private ISocketFactory _socketFactory;
        private LobbyClient _lobbyClient;
        private WorldClient _worldClient;
        private ClientState _state;
        private IGenericTCS _activeTask;
        private List<CharacterInfo> _characters;
        private Dictionary<int, DistrictInfo> _districtMap;

        public APBClient(string username, string password, HardwareStore hw, ISocketFactory socketFactory = null)
        {
            _socketFactory = socketFactory;
            SetupLobbyClient(username, password, hw);
        }

        private EventHandler<T> GenerateEventHandler<T>(EventHandler<T> handler)
        {
            RequiredStateAttribute attribute = (RequiredStateAttribute)handler.Method.GetCustomAttribute(typeof(RequiredStateAttribute));
            if (attribute != null)
            {
                return (sender, e) =>
                {
                    if (EnsureState(attribute.RequiredState, $"{handler.Method.Name} called in unexpected state (Expected = {attribute.RequiredState}, Actual = {_state})"))
                    {
                        Log.Debug(handler.Method.Name);
                        handler(sender, e);
                    }
                };
            }
            else
            {
                return handler;
            }
        }

        private bool EnsureState(ClientState requiredState, string errMessage)
        {
            if (_state != requiredState)
            {
                Log.Warn(errMessage);
                _activeTask.SetTaskException(new Exception(errMessage));
                Disconnect();
                return false;
            }

            return true;
        }

        public void Disconnect()
        {
            _state = ClientState.Disconnected;
            _lobbyClient?.Disconnect();
            _worldClient?.Disconnect();
        }

        #region Lobby Client
        private void SetupLobbyClient(string username, string password, HardwareStore hw)
        {
            _lobbyClient = new LobbyClient(username, password, hw, _socketFactory);
            _lobbyClient.OnConnectSuccess += GenerateEventHandler<EventArgs>(HandleLobbyConnectSuccess);
            _lobbyClient.OnDisconnect += GenerateEventHandler<EventArgs>(HandleLobbyDisconnect);
            _lobbyClient.OnLoginSuccess += GenerateEventHandler<EventArgs>(HandleLoginSuccess);
            _lobbyClient.OnCharacterList += GenerateEventHandler<List<CharacterInfo>>(HandleCharacterList);
            _lobbyClient.OnGetWorldListSuccess += GenerateEventHandler<List<WorldInfo>>(HandleWorldListSuccess);
            _lobbyClient.OnGetWorldListFailed += GenerateEventHandler<int>(HandleWorldListFailed);
            _lobbyClient.OnWorldEnterSuccess += GenerateEventHandler<WorldEnterData>(HandleLobbyWorldEnterSuccess);
            _characters = null;
        }

        private void HandleLobbyDisconnect(object sender, EventArgs e)
        {
            if (_state >= ClientState.LobbyServerWorldEnterComplete)
            {
                return; // We expect lobby to disconnect here and we don't care
            }

            _state = ClientState.Disconnected;
            _activeTask.SetTaskException(new Exception("Connection closed while processing"));
        }

        [RequiredState(ClientState.LobbyServerConnectInProgress)]
        private void HandleLobbyConnectSuccess(object sender, EventArgs e)
        {
            _state = ClientState.LobbyServerLoginInProgress;
        }

        [RequiredState(ClientState.LobbyServerLoginInProgress)]
        private void HandleLoginSuccess(object sender, EventArgs e)
        {
            _state = ClientState.LobbyServerLoginComplete;
        }

        [RequiredState(ClientState.LobbyServerLoginComplete)]
        private void HandleCharacterList(object sender, List<CharacterInfo> e)
        {
            _state = ClientState.LobbyServerCharacterListReceived;
            _characters = e;
            _activeTask?.SetResult(null);
        }

        [RequiredState(ClientState.LobbyServerWorldListInProgress)]
        private void HandleWorldListSuccess(object sender, List<WorldInfo> e)
        {
            _state = ClientState.LobbyServerCharacterListReceived;
            _activeTask?.SetResult(e);
        }

        [RequiredState(ClientState.LobbyServerWorldListInProgress)]
        private void HandleWorldListFailed(object sender, int e)
        {
            _state = ClientState.LobbyServerCharacterListReceived;
            _activeTask?.SetTaskException(new Exception($"Failed to retrieve world list (Return code = {e})"));
        }

        [RequiredState(ClientState.LobbyServerWorldEnterInProgress)]
        private void HandleLobbyWorldEnterSuccess(object sender, WorldEnterData e)
        {
            _state = ClientState.LobbyServerWorldEnterComplete;
            SetupWorldClient(_lobbyClient.GetEncryptionKey(), _lobbyClient.GetAccountId(), e.Timestamp);
            _state = ClientState.WorldServerConnectInProgress;
           _worldClient.Connect(e.WorldServerIpAddress.ToString(), e.WorldServerPort);
        }

        public Task Login()
        {
            if (_state != ClientState.Disconnected)
            {
                throw new InvalidOperationException("Client not in disconnected state");
            }

            var tcs = new VirtualTCS<object>();
            _activeTask = tcs;
            _state = ClientState.LobbyServerConnectInProgress;
            _lobbyClient.Connect(LobbyHost, LobbyPort);

            return tcs.Task;
        }

        public List<CharacterInfo> GetCharacters()
        {
            if (_characters == null)
            {
                throw new InvalidOperationException("Client has not received characters yet");
            }

            return _characters;
        }

        public Task<List<WorldInfo>> GetWorlds()
        {
            if (_state != ClientState.LobbyServerCharacterListReceived)
            {
                throw new InvalidOperationException("Client has not received characters");
            }

            var tcs = new VirtualTCS<List<WorldInfo>>();
            _activeTask = tcs;
            _state = ClientState.LobbyServerWorldListInProgress;
            _lobbyClient.GetWorldList();
            return tcs.Task;
        }

        public Task<FinalWorldEnterData> EnterWorld(int characterSlotNumber)
        {
            if (_state != ClientState.LobbyServerCharacterListReceived)
            {
                throw new InvalidOperationException("Client has not received characters or busy");
            }

            var tcs = new VirtualTCS<FinalWorldEnterData>();
            _activeTask = tcs;
            _state = ClientState.LobbyServerWorldEnterInProgress;
            _lobbyClient.EnterWorld(characterSlotNumber);
            return tcs.Task;
        }
        #endregion

        #region World Client
        private void SetupWorldClient(byte[] encryptionKey, uint accountId, ulong timestamp)
        {
            _worldClient = new WorldClient(encryptionKey, accountId, timestamp, _socketFactory);
            _worldClient.OnConnectSuccess += GenerateEventHandler<EventArgs>(HandleWorldConnectSuccess);
            _worldClient.OnDisconnect += GenerateEventHandler<EventArgs>(HandleWorldDisconnect);
            _worldClient.OnWorldEnterSuccess += GenerateEventHandler<FinalWorldEnterData>(HandleWorldEnterSuccess);
            _worldClient.OnInstanceListSuccess += GenerateEventHandler<List<InstanceInfo>>(HandleInstanceListSuccess);
            _worldClient.OnDistrictListSuccess += GenerateEventHandler<List<DistrictInfo>>(HandleDistrictListSuccess);
            _worldClient.OnDistrictReserveSuccess += GenerateEventHandler<ReserveInfo>(HandleDistrictReserveSuccess);
            _worldClient.OnDistrictReserveFailed += GenerateEventHandler<int>(HandleDistrictReserveFailed);
            _worldClient.OnDistrictEnterSuccess += GenerateEventHandler<DistrictEnterInfo>(HandleDistrictEnterSuccess);
            _worldClient.OnDistrictEnterFailed += GenerateEventHandler<int>(HandleDistrictEnterFailed);
            _districtMap = null;
        }

        private void HandleWorldDisconnect(object sender, EventArgs e)
        {
            _state = ClientState.Disconnected;
            _activeTask?.SetTaskException(new Exception("Connection closed while processing"));
        }

        [RequiredState(ClientState.WorldServerConnectInProgress)]
        private void HandleWorldConnectSuccess(object sender, EventArgs e)
        {
            _state = ClientState.WorldServerConnectComplete;
        }

        [RequiredState(ClientState.WorldServerConnectComplete)]
        private void HandleDistrictListSuccess(object sender, List<DistrictInfo> e)
        {
            _districtMap = new Dictionary<int, DistrictInfo>();

            foreach (DistrictInfo district in e)
            {
                _districtMap[district.DistrictUid] = district;
            }
        }

        [RequiredState(ClientState.WorldServerConnectComplete)]
        private void HandleWorldEnterSuccess(object sender, FinalWorldEnterData e)
        {
            _state = ClientState.WorldServerWorldEnterComplete;
            _activeTask?.SetResult(e);
        }

        [RequiredState(ClientState.WorldServerInstanceListInProgress)]
        private void HandleInstanceListSuccess(object sender, List<InstanceInfo> e)
        {
            _state = ClientState.WorldServerWorldEnterComplete;
            _activeTask?.SetResult(e);
        }

        [RequiredState(ClientState.WorldServerDistrictReserveInProgress)]
        private void HandleDistrictReserveSuccess(object sender, ReserveInfo reserveInfo)
        {
            _worldClient.AskDistrictEnter();
            _state = ClientState.WorldServerDistrictEnterInProgress;
        }

        [RequiredState(ClientState.WorldServerDistrictReserveInProgress)]
        private void HandleDistrictReserveFailed(object sender, int e)
        {
            _state = ClientState.WorldServerWorldEnterComplete;
            _activeTask?.SetTaskException(new Exception($"Failed to reserve district (Error Code = {e})"));
        }

        [RequiredState(ClientState.WorldServerDistrictEnterInProgress)]
        private void HandleDistrictEnterSuccess(object sender, DistrictEnterInfo enterInfo)
        {
            _state = ClientState.WorldServerDistrictEnterComplete;
            _activeTask?.SetResult(enterInfo);
        }

        [RequiredState(ClientState.WorldServerDistrictEnterInProgress)]
        private void HandleDistrictEnterFailed(object send, int e)
        {
            _state = ClientState.WorldServerWorldEnterComplete;
            _activeTask?.SetTaskException(new Exception($"Failed to enter district (Error Code = {e})"));
        }

        public Dictionary<int, DistrictInfo> GetDistricts()
        {
            if (_districtMap == null)
            {
                throw new InvalidOperationException("Client has not entered world yet");
            }

            return _districtMap;
        }

        public Task<List<InstanceInfo>> GetInstances()
        {
            if (_state != ClientState.WorldServerWorldEnterComplete)
            {
                throw new InvalidOperationException("Client has not entered world");
            }

            var tcs = new VirtualTCS<List<InstanceInfo>>();
            _activeTask = tcs;
            _state = ClientState.WorldServerInstanceListInProgress;
            _worldClient.GetInstanceList();
            return tcs.Task;
        }

        public Task<DistrictEnterInfo> JoinInstance(InstanceInfo instance)
        {
            if (_state != ClientState.WorldServerWorldEnterComplete)
            {
                throw new InvalidOperationException("Client has not entered world");
            }

            var tcs = new VirtualTCS<DistrictEnterInfo>();
            _activeTask = tcs;
            _state = ClientState.WorldServerDistrictReserveInProgress;
            _worldClient.AskDistrictReserve(instance.DistrictUid, instance.InstanceNum, -1, false);
            return tcs.Task;
        }
        #endregion
    }
}
