using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using APBWatcher.IO;

namespace APBWatcher.Networking
{
    public class ServerPacket
    {
        public byte[] Data { get; }
        public APBBinaryReader Reader { get; }
        public uint OpCode { get; }

        // Expected that the size has alredy been dealt with by the processor, the first 32 bits should be the opcode
        public ServerPacket(byte[] dataBuf, int bufOffset, int size)
        {
            Data = new byte[size];
            Buffer.BlockCopy(dataBuf, bufOffset, Data, 0, size);

            Reader = new APBBinaryReader(new MemoryStream(Data));
            OpCode = Reader.ReadUInt32();
        }
    }
}
