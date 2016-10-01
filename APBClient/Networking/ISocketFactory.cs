using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Org.Mentalis.Network.ProxySocket;

namespace APBClient.Networking
{
    public interface ISocketFactory
    {
        ProxySocket CreateSocket();
    }
}
