using Castor.Emulator.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public static class BytewiseOperations
    {
        public static byte MostSignificantByte(this ushort d16)
        {
            return Convert.ToByte((d16 >> 8) & 0xFF);
        }

        public static byte LeastSignificantByte(this ushort d16)
        {
            return Convert.ToByte((d16 >> 0) & 0xFF);
        }

        // Bitwise operations on bytes
        public static bool CheckBit(this byte d8, BitFlags bit) => (d8 & (byte)bit) == (byte)bit;
        public static void SetBit(this byte d8, BitFlags bit) => d8 |= (byte)bit;
        public static void ClearBit(this byte d8, BitFlags bit) => d8 &= (byte)~bit;
        public static void ToggleBit(this byte d8, BitFlags bit) => d8 ^= (byte)bit;

        public static byte BitValue(this byte d8, int index)
        {
            return (byte)((d8 >> index) & 1);
        }
    }
}
