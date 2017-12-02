using Castor.Emulator.CPU;
using Castor.Emulator.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public static class Math
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

        public static void ClearBit(this byte d8, int index)
        {
            d8 = (byte)((d8 & ~(1 << index)) & 0xFF);
        }

        public static void RotateLeft(ref byte d8, ref byte flagRegister)
        {
            byte bit7_d8 = d8.BitValue(7);
            byte carry_bit = flagRegister.BitValue(4); // index 4 is carry flag

            if (bit7_d8 == 0)
                flagRegister &= (byte)~StatusFlags.C;
            else
                flagRegister |= (byte)StatusFlags.C;

            d8 <<= 1;

            d8 |= carry_bit;
        }

        public static byte Swap(this byte d8)
        {
            var hi = (d8 >> 4) & 0xF;
            var lo = (d8 >> 0) & 0xF;

            return (byte)(lo << 4 | hi);
        }

        public static byte Xor(byte d1, byte d2)
        {
            return (byte)(d1 ^ d2);
        }

        public static class Add
        {
            public static bool CheckHalfCarry(ushort val1, params int[] val2)
            {
                return ((val1 & 0xFF) + (val2.Sum() & 0xFF) > 0xFF);
            }

            public static bool CheckFullCarry(ushort val1, params int[] val2)
            {
                return (val1 + val2.Sum()) > ushort.MaxValue;
            }

            public static bool CheckHalfCarry(byte val1, params int[] val2)
            {
                return ((val1 & 0xF) + (val2.Sum() & 0xF) > 0xF);
            }

            public static bool CheckFullCarry(byte val1, params int[] val2)
            {
                return (val1 + val2.Sum()) > byte.MaxValue;
            }

            public static bool CheckZero(byte val1, params int[] val2)
            {
                return (val1 + val2.Sum()) == byte.MaxValue + 1;
            }
        }
    }
}
