using Castor.Emulator.CPU;
using Castor.Emulator.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public static class Bitwise
    {
        public static byte MostSignificantByte(this ushort d16)
        {
            return Convert.ToByte((d16 >> 8) & 0xFF);
        }

        public static byte LeastSignificantByte(this ushort d16)
        {
            return Convert.ToByte((d16 >> 0) & 0xFF);
        }

        public static byte BitValue(this byte d8, int index)
        {
            return (byte)((d8 >> index) & 1);
        }

        public static void SetBit(this byte d8, int index)
        {
            d8 = (byte)((d8 | 1 << index) & 0xFF);
        }

        public static void RotateLeft(ref byte d8, ref byte flagRegister)
        {
            byte bit7_d8 = d8.BitValue(7);
            byte carry_bit = flagRegister.BitValue(4); // index 4 is carry flag

            if (bit7_d8 == 0)
                flagRegister &= (byte)~StatusFlags.CarryFlag;
            else
                flagRegister |= (byte)StatusFlags.CarryFlag;

            d8 <<= 1;

            d8 |= carry_bit;
        }
    }
}
