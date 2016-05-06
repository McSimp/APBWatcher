using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace APBWatcher
{
    class ServerPacket
    {
        private byte[] m_data;
        private APBBinaryReader m_reader;
        private uint m_opcode;

        public byte[] Data { get { return m_data; } }
        public APBBinaryReader Reader { get { return m_reader; } }
        public uint OpCode { get { return m_opcode; } }

        // Expected that the size has alredy been dealt with by the processor, the first 32 bits should be the opcode
        public ServerPacket(byte[] dataBuf, int bufOffset, int size)
        {
            m_data = new byte[size];
            Buffer.BlockCopy(dataBuf, bufOffset, m_data, 0, size);

            m_reader = new APBBinaryReader(new MemoryStream(m_data));
            m_opcode = m_reader.ReadUInt32();
        }
    }
}
