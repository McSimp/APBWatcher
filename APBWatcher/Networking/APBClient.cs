using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.Mentalis.Network.ProxySocket;

namespace APBWatcher.Networking
{
    abstract class APBClient
    {
        private const int RecvBufferSize = 65535;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ProxySocket _socket;
        private byte[] _recvBuffer = new byte[RecvBufferSize];
        private int _receivedLength = 0;
        private NetworkRc4 _encryption = new NetworkRc4();

        public event EventHandler OnConnectSuccess = delegate { };
        public event EventHandler<Exception> OnConnectFailed = delegate { };
        public event EventHandler OnDisconnect = delegate { };

        private void ConnectInternal(string host, int port)
        {
            Log.Info($"Connecting to {host}:{port}");
            _socket.BeginConnect(host, port, ConnectCallback, null);
        }

        public void Connect(string host, int port)
        {
            _socket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectInternal(host, port);
        }

        public void ConnectProxy(string host, int port, string proxyIP, int proxyPort, string proxyUsername, string proxyPassword)
        {
            _socket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.ProxyEndPoint = new IPEndPoint(IPAddress.Parse(proxyIP), proxyPort);
            _socket.ProxyType = ProxyTypes.Socks5;
            if (proxyUsername != null && proxyPassword != null)
            {
                _socket.ProxyUser = proxyUsername;
                _socket.ProxyPass = proxyPassword;
            }

            ConnectInternal(host, port);
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
                BeginReceive();
            }
            catch (Exception e)
            {
                Log.Error("Failed to connect", e);
                OnConnectFailed(this, e);
            }
        }

        protected void PostConnect()
        {

        }

        private void BeginReceive()
        {
            _socket.BeginReceive(_recvBuffer, _receivedLength, _recvBuffer.Length - _receivedLength, SocketFlags.None, ReceiveCallback, null);
        }

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
            catch (Exception e)
            {
                Log.Warn("Error occurred while disconnecting from socket", e);
            }

            OnDisconnect(this, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int length = _socket.EndReceive(ar);
                if (length <= 0)
                {
                    Log.Warn($"Received invalid packet length {length}, disconnecting");
                    Disconnect();
                    return;
                }

                Log.Debug($"Received packet, length={length}");
                _receivedLength += length;

                TryParsePacket();

                if (_socket != null)
                {
                    BeginReceive();
                }
            }
            catch (Exception e)
            {
                Log.Warn("Exception occurred while receiving, disconnecting", e);
                Disconnect();
            }
        }

        private void TryParsePacket()
        {
            int size = BitConverter.ToInt32(_recvBuffer, 0);
            if (size > _receivedLength)
            {
                Log.Debug($"Not enough data to construct packet (Have {_receivedLength}, need {size})");
                return;
            }

            // Construct new packet
            Log.Debug($"Size field = {size}");

            // Decrypt packet if need be
            if (_encryption.Initialized)
            {
                _encryption.DecryptServerData(_recvBuffer, 4, size - 4);
            }

            var packet = new ServerPacket(_recvBuffer, 4, size - 4);
            _receivedLength -= size;

            HandlePacket(packet);
        }

        protected abstract void HandlePacket(ServerPacket packet);

        public void SetEncryptionKey(byte[] key)
        {
            _encryption.SetKey(key);
        }

        public void SendPacket(ClientPacket packet)
        {
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
    }
}
