using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Org.Mentalis.Network.ProxySocket;

namespace APBClient.Networking
{
    public class ProxySocketFactory : ISocketFactory
    {
        private string _proxyIP;
        private int _proxyPort;
        private string _proxyUsername;
        private string _proxyPassword;

        public ProxySocketFactory(string proxyIP, int proxyPort, string proxyUsername, string proxyPassword)
        {
            _proxyIP = proxyIP;
            _proxyPort = proxyPort;
            _proxyUsername = proxyUsername;
            _proxyPassword = proxyPassword;
        }

        public ProxySocket CreateSocket()
        {
            var socket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.ProxyEndPoint = new IPEndPoint(IPAddress.Parse(_proxyIP), _proxyPort);
            socket.ProxyType = ProxyTypes.Socks5;
            if (_proxyUsername != null && _proxyPassword != null)
            {
                socket.ProxyUser = _proxyUsername;
                socket.ProxyPass = _proxyPassword;
            }

            return socket;
        }
    }
}
