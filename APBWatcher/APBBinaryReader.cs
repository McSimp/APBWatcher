using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace APBWatcher
{
    public class APBBinaryReader : BinaryReader
    {
        public APBBinaryReader(Stream input) : base(input) { }

        private char ReadUnicodeChar()
        {
            if (BaseStream.Position + 2 > BaseStream.Length)
            {
                return char.MinValue;
            }

            return BitConverter.ToChar(ReadBytes(2), 0);
        }

        public string ReadUnicodeString()
        {
            StringBuilder sb = new StringBuilder();

            for (char ch = ReadUnicodeChar(); ch != char.MinValue; ch = ReadUnicodeChar())
            {
                sb.Append(ch);
            }

            return sb.ToString();
        }

        public string ReadUnicodeString(int fieldSize)
        {
            StringBuilder sb = new StringBuilder();
            long start = BaseStream.Position;
            for (char ch = ReadUnicodeChar(); ch != char.MinValue && (start - BaseStream.Position) < fieldSize; ch = ReadUnicodeChar())
            {
                sb.Append(ch);
            }

            BaseStream.Seek(start + fieldSize, SeekOrigin.Begin);

            return sb.ToString();
        }

        public string ReadASCIIString()
        {
            StringBuilder sb = new StringBuilder();

            for (byte ch = ReadByte(); ch != 0; ch = ReadByte())
            {
                sb.Append(Convert.ToChar(ch));
            }

            return sb.ToString();
        }

        public string ReadASCIIString(int fieldSize)
        {
            StringBuilder sb = new StringBuilder();
            long start = BaseStream.Position;
            for (byte ch = ReadByte(); ch != 0 && (start - BaseStream.Position) < fieldSize; ch = ReadByte())
            {
                sb.Append(Convert.ToChar(ch));
            }

            BaseStream.Seek(start + fieldSize, SeekOrigin.Begin);

            return sb.ToString();
        }
    }
}
