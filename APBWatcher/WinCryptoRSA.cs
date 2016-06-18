using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APBWatcher
{
    class WinCryptoRSA
    {
        public static RsaKeyParameters ReadPublicKeyBlob(BinaryReader reader)
        {
            // Start with BLOBHEADER
            byte type = reader.ReadByte();
            byte version = reader.ReadByte();
            reader.ReadUInt16(); // Skip reserved word
            uint algId = reader.ReadUInt32();

            if (type != 6 || version != 2 || algId != 0x0000A400)
            {
                throw new Exception(String.Format("Unexpected public key header (Type = {0}, Version = {1}, AlgId = {2})", type, version, algId));
            }

            // Read the RSAPUBKEY part
            byte[] magic = reader.ReadBytes(4);
            if (magic[0] != 0x52 || magic[1] != 0x53 || magic[2] != 0x41 || magic[3] != 0x31)
            {
                throw new Exception(String.Format("Incorrect RSAPUBKEY magic ({0}, {1}, {2}, {3})", magic[0], magic[1], magic[2], magic[3]));
            }

            uint bitLength = reader.ReadUInt32();

            // Read the data part
            byte[] exponent = reader.ReadBytes(4);
            byte[] modulus = reader.ReadBytes((int)bitLength / 8);
            
            Array.Reverse(exponent); // MS CryptoAPI uses little endian, Bouncy Castle uses big endian
            Array.Reverse(modulus);

            var pubKey = new RsaKeyParameters(false, new BigInteger(1, modulus), new BigInteger(1, exponent));
            return pubKey;
        }

        public static byte[] CreatePublicKeyBlob(RsaKeyParameters publicKey)
        {
            // byte Type, byte Version, uint16 Reserved, uint32 AlgId, byte Magic[4]
            byte[] header = {
                0x06, 0x02, 0x00, 0x00, 0x00, 0xA4, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31
            };

            byte[] bitLength = BitConverter.GetBytes(publicKey.Modulus.BitLength);
            byte[] exponent = publicKey.Exponent.ToByteArrayUnsigned();
            byte[] modulus = publicKey.Modulus.ToByteArrayUnsigned();

            // bitLength and exponent must go into fields of size 4
            int blobSize = header.Length + 4 + 4 + modulus.Length;
            byte[] blob = new byte[blobSize];

            Array.Reverse(exponent);
            Array.Reverse(modulus);

            Array.Clear(blob, 0, blob.Length);
            Buffer.BlockCopy(header, 0, blob, 0, header.Length);
            Buffer.BlockCopy(bitLength, 0, blob, 12, bitLength.Length);
            Buffer.BlockCopy(exponent, 0, blob, 16, exponent.Length);
            Buffer.BlockCopy(modulus, 0, blob, 20, modulus.Length);

            return blob;
        }

        public static byte[] EncryptData(Pkcs1Encoding engine, byte[] data)
        {
            // Calculate number of blocks we need to encrypt
            int inputBlockSize = engine.GetInputBlockSize();
            int totalBlocks = (data.Length + inputBlockSize - 1) / inputBlockSize; // Integer division, round up

            // Allocate array for the correct amount of data
            byte[] result = new byte[totalBlocks * engine.GetOutputBlockSize()];

            // So long as there is data left to encrypt, encrypt another block
            int inputOffset = 0;
            int outputOffset = 0;

            while (inputOffset < data.Length)
            {
                int dataToConsume = Math.Min(inputBlockSize, data.Length - inputOffset);

                byte[] encryptedBlock = engine.ProcessBlock(data, inputOffset, dataToConsume);
                Array.Reverse(encryptedBlock);
                Buffer.BlockCopy(encryptedBlock, 0, result, outputOffset, encryptedBlock.Length);

                inputOffset += dataToConsume;
                outputOffset += encryptedBlock.Length;
            }

            return result;
        }
    }
}
