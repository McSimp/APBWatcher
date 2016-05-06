using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace APBWatcher
{
    class EncryptionProvider
    {
        RC4Provider m_serverToClient;
        RC4Provider m_clientToServer;

        public bool Initialized { get { return m_serverToClient != null && m_clientToServer != null; } }

        public void SetKey(byte[] key)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] sha1Key = sha1.ComputeHash(key);

            m_clientToServer = new RC4Provider(sha1Key);
            m_serverToClient = new RC4Provider(sha1Key);
        }

        public void EncryptClientData(byte[] data, int offset, int size)
        {
            m_clientToServer.TransformBlock(data, offset, size);
        }

        public void DecryptServerData(byte[] data, int offset, int size)
        {
            m_serverToClient.TransformBlock(data, offset, size);
        }
    }
}
