using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace APBWatcher.IO
{
    public class APBBinaryWriter : BinaryWriter
    {
        public APBBinaryWriter(Stream output) : base(output) { }

        public void WriteUnicodeString(string str, int fieldSize)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(str);

            int i;
            for (i = 0; i < bytes.Length && i < fieldSize; i++)
            {
                Write(bytes[i]);
            }


            if (i < fieldSize)
            {
                for (; i < fieldSize; i++)
                {
                    Write((byte)0);
                }
            }
        }
    }
}
