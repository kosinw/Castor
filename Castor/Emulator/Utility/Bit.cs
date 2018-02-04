using Castor.Emulator.CPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public static class Bit
    {
        public static byte BitValue(byte d8, int index)
        {
            return (byte)((d8 >> index) & 1);
        }

        public static byte SetBit(byte d8, int index)
        {
            return (byte)(d8 | (1 << index));
        }

        public static byte ClearBit(byte d8, int index)
        {
            return (byte)(d8 & ~(1 << index));
        }

        public static byte MSB(this ushort d16)
        {
            return Convert.ToByte((d16 >> 8) & 0xFF);
        }

        public static byte LSB(this ushort d16)
        {
            return Convert.ToByte((d16 >> 0) & 0xFF);
        }                
    }
}
