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
    public class StandardSocketFactory : ISocketFactory
    {
        public ProxySocket CreateSocket()
        {
            return new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
