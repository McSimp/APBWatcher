using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBClient.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.Mentalis.Network.ProxySocket;

namespace APBClient.Networking
{
    public abstract class BaseClient
    {
        private const int RecvBufferSize = 65535;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ISocketFactory _socketFactory;
        private ProxySocket _socket;
        private byte[] _recvBuffer = new byte[RecvBufferSize];
        private int _nextPacketLength;
        private int _receivedLength;
        private NetworkRc4 _encryption = new NetworkRc4();
        private Dictionary<uint, Tuple<string, Action<BaseClient, ServerPacket>>> _packetHandlers = new Dictionary<uint, Tuple<string, Action<BaseClient, ServerPacket>>>();

        public event EventHandler OnConnectSuccess = delegate { };
        public event EventHandler<Exception> OnConnectFailed = delegate { };
        public event EventHandler OnDisconnect = delegate { };

        public BaseClient(ISocketFactory socketFactory = null)
        {
            if (socketFactory == null)
            {
                _socketFactory = new StandardSocketFactory();
            }
            else
            {
                _socketFactory = socketFactory;
            }
            
            SetupHandlers();
        }

        public void Connect(string host, int port)
        {
            _receivedLength = 0;
            _encryption.Initialized = false;
            _socket = _socketFactory.CreateSocket();
            Log.Info($"Connecting to {host}:{port}");
            _socket.BeginConnect(host, port, ConnectCallback, null);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Finish connecting
                _socket.EndConnect(ar);
                Log.Info("Successfully connected");
                OnConnectSuccess(this, null);
                PostConnect();

                // Start receiving
                BeginReceiveLength();
            }
            catch (Exception e)
            {
                Log.Error("Failed to connect", e);
                OnConnectFailed(this, e);
                Disconnect();
            }
        }

        protected virtual void PostConnect() { }

        public void Disconnect()
        {
            if (_socket == null)
            {
                return;
            }

            try
            {
                _socket.Disconnect(false);
                _socket.Close();
                _socket = null;
            }
            catch (ObjectDisposedException e) {}
            catch (Exception e)
            {
                Log.Warn("Error occurred while disconnecting from socket", e);
            }

            OnDisconnect(this, null);
        }

        private void BeginReceiveLength()
        {
            _socket.BeginReceive(_recvBuffer, _receivedLength, 4 - _receivedLength, SocketFlags.None, ReceiveLengthCallback, null);
        }

        private void BeginReceiveData()
        {
            _socket.BeginReceive(_recvBuffer, _receivedLength, _nextPacketLength - _receivedLength, SocketFlags.None, ReceiveDataCallback, null);
        }

        private void ReceiveLengthCallback(IAsyncResult ar)
        {
            if (_socket == null)
            {
                return;
            }

            try
            {
                int length = _socket.EndReceive(ar);
                if (length <= 0)
                {
                    Log.Warn($"Received invalid packet length {length}, disconnecting");
                    Disconnect();
                    return;
                }

                _receivedLength += length;

                if (_receivedLength == 4)
                {
                    _nextPacketLength = BitConverter.ToInt32(_recvBuffer, 0) - 4;
                    Log.Debug($"Received length {_nextPacketLength}");
                    _receivedLength = 0;
                    BeginReceiveData();
                    // TODO: Ensure not bigger than recv buffer
                }
                else
                {
                    BeginReceiveLength();
                }
            }
            catch (Exception e)
            {
                Log.Warn("Exception occurred while receiving, disconnecting", e);
                Disconnect();
            }
        }

        private void ReceiveDataCallback(IAsyncResult ar)
        {
            if (_socket == null)
            {
                return;
            }

            try
            {
                int length = _socket.EndReceive(ar);
                if (length <= 0)
                {
                    Log.Warn($"Received invalid packet length {length}, disconnecting");
                    Disconnect();
                    return;
                }

                Log.Debug($"Received data packet, length={length}");
                _receivedLength += length;

                if (_receivedLength == _nextPacketLength)
                {
                    TryParsePacket();
                    BeginReceiveLength();
                }
                else
                {
                    BeginReceiveData();
                }
            }
            catch (Exception e)
            {
                Log.Warn("Exception occurred while receiving, disconnecting", e);
                Disconnect();
            }
        }

        private bool TryParsePacket()
        {
            // Decrypt packet if need be
            if (_encryption.Initialized)
            {
                _encryption.DecryptServerData(_recvBuffer, 0, _receivedLength);
            }

            var packet = new ServerPacket(_recvBuffer, 0, _receivedLength);
            _receivedLength = 0;
            HandlePacket(packet);

            return true;
        }

        protected void HandlePacket(ServerPacket packet)
        {
            if (_packetHandlers.ContainsKey(packet.OpCode))
            {
                var handlerData = _packetHandlers[packet.OpCode];
                Log.Info($"Receive [{handlerData.Item1}]");
                handlerData.Item2(this, packet);
            }
            else
            {
                Log.Warn($"Unknown packet received (Opcode = {packet.OpCode})");
            }
        }

        protected void SetEncryptionKey(byte[] key)
        {
            _encryption.SetKey(key);
        }

        protected void SendPacket(ClientPacket packet)
        {
            Log.Info($"Sending [{packet.GetType().Name}]");

            byte[] data = packet.GetDataForSending();

            //Log.Debug("Raw packet data:" + Environment.NewLine + HexDump(data));

            // Encrypt the packet if needed
            if (_encryption.Initialized)
            {
                _encryption.EncryptClientData(data, 4, packet.TotalSize - 4); // Don't encrypt size
                //Log.Debug("Encrypted packet data:" + Environment.NewLine + HexDump(data));
            }

            _socket.Send(data, 0, packet.TotalSize, SocketFlags.None); // TODO: Make async
        }

        private void SetupHandlers()
        {
            Type[] types = GetType().GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
            foreach (Type type in types)
            {
                if (!type.IsClass) continue;
                if (!type.GetInterfaces().Contains(typeof(IPacketHandler))) continue;

                PacketHandlerAttribute attribute = (PacketHandlerAttribute)type.GetCustomAttribute(typeof(PacketHandlerAttribute));
                if (attribute == null) continue;

                uint opCode = (uint)attribute.OpCode;

                var handlerInstance = Activator.CreateInstance(type);
                var handlerDel = (Action<BaseClient, ServerPacket>)Delegate.CreateDelegate(typeof(Action<BaseClient, ServerPacket>), handlerInstance, "HandlePacket");

                _packetHandlers.Add(opCode, Tuple.Create(type.Name, handlerDel));

                Log.Debug($"Registered handler for {type.Name} (OpCode = {opCode})");
            }
        }
    }
}
