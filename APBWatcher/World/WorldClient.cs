using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using APBWatcher.Lobby;
using Org.BouncyCastle.Crypto.Encodings;
using APBWatcher.Networking;
using Org.BouncyCastle.Crypto.Digests;

namespace APBWatcher.World
{
    public partial class WorldClient : BaseClient
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<ErrorData> OnError = delegate { };
        public event EventHandler<KickData> OnKick = delegate { };

        private byte[] _encryptionKey;
        private uint _accountId;
        private ulong _timestamp;

        public WorldClient(byte[] encryptionKey, uint accountId, ulong timestamp)
        {
            _encryptionKey = encryptionKey;
            _accountId = accountId;
            _timestamp = timestamp;
        }

        protected override void PostConnect()
        {
            var sha1 = new Sha1Digest();
            sha1.BlockUpdate(_encryptionKey, 0, _encryptionKey.Length);
            sha1.BlockUpdate(BitConverter.GetBytes(_timestamp), 0, 8);
            var hash = new byte[sha1.GetDigestSize()];
            sha1.DoFinal(hash, 0);

            var req = new GC2WS_ASK_WORLD_ENTER(_accountId, hash);
            SendPacket(req);

            SetEncryptionKey(_encryptionKey);
        }

        public void GetInstanceList()
        {
            var instanceListReq = new GC2WS_ASK_INSTANCE_LIST();
            SendPacket(instanceListReq);
        }
    }
}

