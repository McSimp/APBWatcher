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
            LobbyServerConnectComplete,
            LobbyServerLoginInProgress, // IGNORED
            LobbyServerLoginComplete,
            LobbyServerCharacterListReceived,
            LobbyServerWorldEnterInProgress,
            LobbyServerWorldEnterComplete,
            WorldServerConnectInProgress,
            WorldServerConnectComplete,
            WorldServerWorldEnterInProgress, // IGNORED
            WorldServerWorldEnterComplete,
        }

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string LobbyHost = "apb.login.gamersfirst.com";
        private const int LobbyPort = 1001;

        private ISocketFactory _socketFactory;
        private LobbyClient _lobbyClient;
        private WorldClient _worldClient;
        private ClientState _state;
        private bool _busy;
        private TaskCompletionSource<object> _activeLoginTask;
        private TaskCompletionSource<List<WorldInfo>> _activeWorldTask;
        private TaskCompletionSource<FinalWorldEnterData> _activeWorldEnterTask;
        private TaskCompletionSource<List<InstanceInfo>> _activeInstanceTask;
        private List<CharacterInfo> _characters;
        private Dictionary<int, DistrictInfo> _districtMap;

        public APBClient(string username, string password, HardwareStore hw, ISocketFactory socketFactory = null)
        {
            _socketFactory = socketFactory;
            _lobbyClient = new LobbyClient(username, password, hw, _socketFactory);
            _lobbyClient.OnConnectSuccess += GenerateEventHandler(HandleLobbyConnectSuccess);
            _lobbyClient.OnDisconnect += GenerateEventHandler(HandleLobbyDisconnect);
            _lobbyClient.OnLoginSuccess += GenerateEventHandler(HandleLoginSuccess);
            _lobbyClient.OnCharacterList += GenerateEventHandler<List<CharacterInfo>>(HandleCharacterList);
            _lobbyClient.OnGetWorldListSuccess += GenerateEventHandler<List<WorldInfo>>(HandleWorldListSuccess);
            _lobbyClient.OnWorldEnterSuccess += GenerateEventHandler<WorldEnterData>(HandleLobbyWorldEnterSuccess);
        }

        private void SetupWorldClient(byte[] encryptionKey, uint accountId, ulong timestamp)
        {
            _worldClient = new WorldClient(encryptionKey, accountId, timestamp, _socketFactory);
            _worldClient.OnConnectSuccess += GenerateEventHandler(HandleWorldConnectSuccess);
            _worldClient.OnDisconnect += GenerateEventHandler(HandleWorldDisconnect);
            _worldClient.OnWorldEnterSuccess += GenerateEventHandler<FinalWorldEnterData>(HandleWorldEnterSuccess);
            _worldClient.OnInstanceListSuccess += GenerateEventHandler<List<InstanceInfo>>(HandleInstanceListSuccess);
            _worldClient.OnDistrictListSuccess += GenerateEventHandler<List<DistrictInfo>>(HandleDistrictListSuccess);
            _districtMap = new Dictionary<int, DistrictInfo>();
        }

        private EventHandler GenerateEventHandler(EventHandler handler)
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
                _busy = false;
                SetAllTaskExceptions(errMessage);
                Disconnect();
                return false;
            }

            return true;
        }

        private void SetTaskException<T>(TaskCompletionSource<T> tcs, string message)
        {
            Task task = tcs?.Task;
            if (task?.Status == TaskStatus.WaitingForActivation)
            {
                tcs.SetException(new Exception(message));
            }
        }

        private void SetAllTaskExceptions(string message)
        {
            SetTaskException(_activeLoginTask, message);
            SetTaskException(_activeWorldTask, message);
            SetTaskException(_activeWorldEnterTask, message);
            SetTaskException(_activeInstanceTask, message);
        }

        private void HandleLobbyDisconnect(object sender, EventArgs e)
        {
            if (_state >= ClientState.LobbyServerWorldEnterComplete)
            {
                return; // We expect lobby to disconnect here
            }

            _state = ClientState.Disconnected;
            _busy = false;
            SetAllTaskExceptions("Connection closed while processing");
        }

        public void Disconnect()
        {
            _state = ClientState.Disconnected;
            _lobbyClient?.Disconnect();
            _worldClient?.Disconnect();
        }

        [RequiredState(ClientState.LobbyServerConnectInProgress)]
        private void HandleLobbyConnectSuccess(object sender, EventArgs e)
        {
            _state = ClientState.LobbyServerConnectComplete;
        }

        [RequiredState(ClientState.LobbyServerConnectComplete)]
        private void HandleLoginSuccess(object sender, EventArgs e)
        {
            _state = ClientState.LobbyServerLoginComplete;
        }

        [RequiredState(ClientState.LobbyServerLoginComplete)]
        private void HandleCharacterList(object sender, List<CharacterInfo> e)
        {
            _state = ClientState.LobbyServerCharacterListReceived;
            _characters = e;
            _busy = false;
            _activeLoginTask?.SetResult(null);
        }

        [RequiredState(ClientState.LobbyServerCharacterListReceived)]
        private void HandleWorldListSuccess(object sender, List<WorldInfo> e)
        {
            _busy = false;
            _activeWorldTask?.SetResult(e);
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
            if (_state != ClientState.Disconnected || _busy)
            {
                throw new InvalidOperationException("Client not in disconnected state or busy");
            }

            _activeLoginTask = new TaskCompletionSource<object>();
            _busy = true;

            _state = ClientState.LobbyServerConnectInProgress;
            _lobbyClient.Connect(LobbyHost, LobbyPort);

            return _activeLoginTask.Task;
        }

        public List<CharacterInfo> GetCharacters()
        {
            if (_state != ClientState.LobbyServerCharacterListReceived)
            {
                throw new InvalidOperationException("Client has not received characters yet");
            }

            return _characters;
        }

        public Task<List<WorldInfo>> GetWorlds()
        {
            if (_state != ClientState.LobbyServerCharacterListReceived || _busy)
            {
                throw new InvalidOperationException("Client has not received characters or busy");
            }

            _activeWorldTask = new TaskCompletionSource<List<WorldInfo>>();
            _busy = true;
            _lobbyClient.GetWorldList();
            return _activeWorldTask.Task;
        }

        public Task<FinalWorldEnterData> EnterWorld(int characterSlotNumber)
        {
            if (_state != ClientState.LobbyServerCharacterListReceived || _busy)
            {
                throw new InvalidOperationException("Client has not received characters or busy");
            }

            _activeWorldEnterTask = new TaskCompletionSource<FinalWorldEnterData>();
            _busy = true;
            _state = ClientState.LobbyServerWorldEnterInProgress;
            _lobbyClient.EnterWorld(characterSlotNumber);
            return _activeWorldEnterTask.Task;
        }

        private void HandleWorldDisconnect(object sender, EventArgs e)
        {
            _state = ClientState.Disconnected;
            _busy = false;
            SetAllTaskExceptions("Connection closed while processing");
        }

        [RequiredState(ClientState.WorldServerConnectInProgress)]
        private void HandleWorldConnectSuccess(object sender, EventArgs e)
        {
            _state = ClientState.WorldServerConnectComplete;
        }

        [RequiredState(ClientState.WorldServerConnectComplete)]
        private void HandleDistrictListSuccess(object sender, List<DistrictInfo> e)
        {
            foreach (DistrictInfo district in e)
            {
                _districtMap[district.DistrictUid] = district;
            }
        }

        [RequiredState(ClientState.WorldServerConnectComplete)]
        private void HandleWorldEnterSuccess(object sender, FinalWorldEnterData e)
        {
            _state = ClientState.WorldServerWorldEnterComplete;
            _busy = false;
            _activeWorldEnterTask?.SetResult(e);
        }

        [RequiredState(ClientState.WorldServerWorldEnterComplete)]
        private void HandleInstanceListSuccess(object sender, List<InstanceInfo> e)
        {
            _busy = false;
            _activeInstanceTask?.SetResult(e);
        }

        public Dictionary<int, DistrictInfo> GetDistricts()
        {
            if (_state != ClientState.WorldServerWorldEnterComplete)
            {
                throw new InvalidOperationException("Client has not entered world yet");
            }

            return _districtMap;
        }

        public Task<List<InstanceInfo>> GetInstances()
        {
            if (_state != ClientState.WorldServerWorldEnterComplete || _busy)
            {
                throw new InvalidOperationException("Client has not entered world or busy");
            }

            _activeInstanceTask = new TaskCompletionSource<List<InstanceInfo>>();
            _busy = true;
            _worldClient.GetInstanceList();
            return _activeInstanceTask.Task;
        }
    }
}
