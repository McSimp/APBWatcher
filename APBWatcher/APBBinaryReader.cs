using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace APBWatcher
{
    class APBBinaryReader : BinaryReader
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
    }
}
