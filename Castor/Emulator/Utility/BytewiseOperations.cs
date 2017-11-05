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

        public static PalletteData[] ToPallette(this byte bgp)
        {
            byte bit7 = bgp.CheckBit(BitFlags.Bit7) ? (byte)1 : (byte)0;
            byte bit6 = bgp.CheckBit(BitFlags.Bit6) ? (byte)1 : (byte)0;
            byte bit5 = bgp.CheckBit(BitFlags.Bit5) ? (byte)1 : (byte)0;
            byte bit4 = bgp.CheckBit(BitFlags.Bit4) ? (byte)1 : (byte)0;
            byte bit3 = bgp.CheckBit(BitFlags.Bit3) ? (byte)1 : (byte)0;
            byte bit2 = bgp.CheckBit(BitFlags.Bit2) ? (byte)1 : (byte)0;
            byte bit1 = bgp.CheckBit(BitFlags.Bit1) ? (byte)1 : (byte)0;
            byte bit0 = bgp.CheckBit(BitFlags.Bit0) ? (byte)1 : (byte)0;

            PalletteData pd3 = (PalletteData)(bit7 << 1 | bit6);
            PalletteData pd2 = (PalletteData)(bit5 << 1 | bit4);
            PalletteData pd1 = (PalletteData)(bit3 << 1 | bit2);
            PalletteData pd0 = (PalletteData)(bit1 << 1 | bit0);

            return new PalletteData[] { pd3, pd2, pd1, pd0 };
        }
    }
}
