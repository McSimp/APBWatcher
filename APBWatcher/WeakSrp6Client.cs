using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace APBWatcher
{
    class WeakSrp6Client : Srp6Client
    {
        protected byte[] identity;
        protected byte[] salt;

        public override BigInteger GenerateClientCredentials(byte[] salt, byte[] identity, byte[] password)
        {
            this.identity = identity;
            this.salt = salt;

            return base.GenerateClientCredentials(salt, identity, password);
        }

        /**
	     * Computes final session key - we don't care that we haven't verified the server for APB.
         * Algorithm is also different - uses mgf1 instead of just a straight hash.
	     * @return Key: the symmetric session key
	     * @throws CryptoException
	     */
        public override BigInteger CalculateSessionKey()
        {
            // Verify pre-requirements (only need S)
            if (this.S == null)
            {
                throw new CryptoException("Impossible to compute Key: " +
                        "some data are missing from the previous operations (S)");
            }

            byte[] output = new byte[40];

            Mgf1BytesGenerator generator = new Mgf1BytesGenerator(digest);
            generator.Init(new MgfParameters(S.ToByteArrayUnsigned()));
            generator.GenerateBytes(output, 0, output.Length);

            this.Key = new BigInteger(1, output);

            return Key;
        }

        /**
	     * Computes the client evidence message M1 using the previously received values.
	     * To be called after calculating the secret S.
	     * @return M1: the client side generated evidence message
	     * @throws CryptoException
	     */
        public override BigInteger CalculateClientEvidenceMessage()
        {
            // Verify pre-requirements
            if (this.pubA == null || this.B == null || this.S == null || this.Key == null)
            {
                throw new CryptoException("Impossible to compute M1: " +
                        "some data are missing from the previous operations (A,B,S,Key)");
            }

            // Compute H(N)
            byte[] hN = new byte[digest.GetDigestSize()];
            byte[] bytesN = N.ToByteArrayUnsigned();
            digest.BlockUpdate(bytesN, 0, bytesN.Length);
            digest.DoFinal(hN, 0);

            // Compute H(g)
            byte[] hg = new byte[digest.GetDigestSize()];
            byte[] bytesg = g.ToByteArrayUnsigned();
            digest.BlockUpdate(bytesg, 0, bytesg.Length);
            digest.DoFinal(hg, 0);

            // Calculate H(N) ^ H(g)
            for (int i = 0; i < hN.Length; i++)
            {
                hN[i] ^= hg[i];
            }

            // Calculate H(identity)
            byte[] identityHash = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(identity, 0, identity.Length);
            digest.DoFinal(identityHash, 0);

            // Calculate H(H(N) ^ H(g) | H(identity) | salt | A | B | Key)
            digest.BlockUpdate(hN, 0, hN.Length);
            digest.BlockUpdate(identityHash, 0, identityHash.Length);
            digest.BlockUpdate(salt, 0, salt.Length);

            byte[] ABytes = pubA.ToByteArrayUnsigned();
            digest.BlockUpdate(ABytes, 0, ABytes.Length);

            byte[] BBytes = B.ToByteArrayUnsigned();
            digest.BlockUpdate(BBytes, 0, BBytes.Length);

            byte[] KeyBytes = Key.ToByteArrayUnsigned();
            digest.BlockUpdate(KeyBytes, 0, KeyBytes.Length);

            // compute the client evidence message 'M1'
            byte[] M1Bytes = new byte[digest.GetDigestSize()];
            digest.DoFinal(M1Bytes, 0);

            this.M1 = new BigInteger(1, M1Bytes);

            return M1;
        }

        protected override BigInteger SelectPrivateValue()
        {
            return new BigInteger(256, random);
        }
    }
}
