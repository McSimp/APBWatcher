using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace APBWatcher
{
    class ClientPacket
    {
        private byte[] m_data;
        private APBBinaryWriter m_writer;
        private int m_totalSize;
        private uint m_opcode;

        public APBBinaryWriter Writer { get { return m_writer; } }
        public int TotalSize { get { return m_totalSize; } }
        public uint OpCode { get { return m_opcode; } protected set { m_opcode = value; } }

        public ClientPacket()
        {

        }

        protected void AllocateData(int size)
        {
            m_data = new byte[size+4];
            m_totalSize = size+4;
            m_writer = new APBBinaryWriter(new MemoryStream(m_data));
            m_writer.Seek(8, SeekOrigin.Begin);
        }

        protected void OverrideSize(int size)
        {
            m_totalSize = size+4;
        }

        public byte[] GetDataForSending()
        {
            m_writer.Seek(0, SeekOrigin.Begin);
            m_writer.Write(m_totalSize);
            m_writer.Write(m_opcode);

            return m_data;
        }
    }
}
