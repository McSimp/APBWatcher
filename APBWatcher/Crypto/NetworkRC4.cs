using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace APBWatcher.Crypto
{
    class NetworkRc4
    {
        private readonly RC4Engine _serverToClient = new RC4Engine();
        private readonly RC4Engine _clientToServer = new RC4Engine();

        public bool Initialized { get; set; }

        public void SetKey(byte[] key)
        {
            var sha1 = new SHA1CryptoServiceProvider();
            byte[] sha1Key = sha1.ComputeHash(key);
            var keyParam = new KeyParameter(sha1Key);

            _clientToServer.Init(true, keyParam);
            _serverToClient.Init(false, keyParam);

            Initialized = true;
        }

        public void EncryptClientData(byte[] data, int offset, int size)
        {
            _clientToServer.ProcessBytes(data, offset, size, data, offset);
        }

        public void DecryptServerData(byte[] data, int offset, int size)
        {
            _serverToClient.ProcessBytes(data, offset, size, data, offset);
        }
    }
}
