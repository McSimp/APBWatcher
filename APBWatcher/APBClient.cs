using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Lobby;
using log4net.Util;

namespace APBWatcher
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
            LobbyServerCharacterListReceived
        }

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string LobbyHost = "apb.login.gamersfirst.com";
        private const int LobbyPort = 1001;

        private LobbyClient _lobbyClient;
        private ClientState _state;
        private bool _busy;
        private TaskCompletionSource<object> _activeLoginTask;
        private TaskCompletionSource<List<WorldInfo>> _activeWorldTask;
        private List<CharacterInfo> _characters;

        public APBClient(string username, string password, string hwFile)
        {
            _lobbyClient = new LobbyClient(username, password, new HardwareStore(hwFile));
            _lobbyClient.OnConnectSuccess += GenerateEventHandler(HandleLobbyConnectSuccess);
            _lobbyClient.OnDisconnect += GenerateEventHandler(HandleLobbyDisconnect);
            _lobbyClient.OnLoginSuccess += GenerateEventHandler(HandleLoginSuccess);
            _lobbyClient.OnCharacterList += GenerateEventHandler<List<CharacterInfo>>(HandleCharacterList);
            _lobbyClient.OnGetWorldListSuccess += GenerateEventHandler<List<WorldInfo>>(HandleWorldListSuccess);
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
                SetTaskException(_activeLoginTask, errMessage);
                SetTaskException(_activeWorldTask, errMessage);
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

        private void HandleLobbyDisconnect(object sender, EventArgs e)
        {
            // CHECK IF WORLD STUFF HAPPENING BECAUSE LOBBY WILL DISCONNECT
            _state = ClientState.Disconnected;
            _busy = false;
            SetTaskException(_activeLoginTask, "Connection closed while processing");
            SetTaskException(_activeWorldTask, "Connection closed while processing");
        }

        private void Disconnect()
        {
            _state = ClientState.Disconnected;
            _lobbyClient.Disconnect();
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

        public Task Login()
        {
            if (_state != ClientState.Disconnected || _busy)
            {
                throw new InvalidOperationException("Client not in disconnected state or busy");    
            }

            _activeLoginTask = new TaskCompletionSource<object>();
            _busy = true;

            _state = ClientState.LobbyServerConnectInProgress;
            _lobbyClient.ConnectProxy(LobbyHost, LobbyPort, "127.0.0.1", 9150, null, null);

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
    }
}
