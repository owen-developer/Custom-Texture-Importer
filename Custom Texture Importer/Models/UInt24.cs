using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom_Texture_Importer.Models
{
    public struct UInt24
    {
        private byte _b0;
        private byte _b1;
        private byte _b2;

        public UInt24(uint value)
        {
            _b0 = (byte)((value) & 0xFF);
            _b1 = (byte)((value >> 8) & 0xFF);
            _b2 = (byte)((value >> 16) & 0xFF);
        }

        public byte[] Bytes { get { return new byte[3] { _b0, _b1, _b2 }; } }
    }
}
