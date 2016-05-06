using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBWatcher
{
    class RC4Provider
    {
        private byte[] S = new byte[256];
        private int stateI = 0;
        private int stateJ = 0;

        public RC4Provider(byte[] key)
        {
            SetKey(key);
        }

        private void KSA(byte[] key)
        {
            for (int i = 0; i < 256; i++)
            {
                S[i] = (byte)i;
            }

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + S[i] + key[i % key.Length]) % 256;

                byte temp = S[i];
                S[i] = S[j];
                S[j] = temp;
            }
        }

        private byte PRGA()
        {
            stateI = (stateI + 1) % 256;
            stateJ = (stateJ + S[stateI]) % 256;

            byte temp = S[stateI];
            S[stateI] = S[stateJ];
            S[stateJ] = temp;

            return S[(S[stateI] + S[stateJ]) % 256];
        }

        public void SetKey(byte[] key)
        {
            KSA(key);
            stateI = 0;
            stateJ = 0;
        }

        public void TransformBlock(byte[] data, int offset, int size)
        {
            for (int i = offset; i < offset + size; i++)
            {
                data[i] ^= PRGA();
            }
        }

        public void SkipBytes(int bytesToSkip)
        {
            for (int i = 0; i < bytesToSkip; i++)
            {
                PRGA();
            }
        }
    }
}
