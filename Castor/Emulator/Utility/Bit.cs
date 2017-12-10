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

        public static byte Value(byte d8, int index, ref byte F)
        {
            var result = (byte)((d8 >> index) & 1);

            AlterFlag(ref F, Cond.Z, result == 0);
            AlterFlag(ref F, Cond.N, false);
            AlterFlag(ref F, Cond.H, true);

            return result;
        }
       
        public static void AlterFlag(ref byte F, Cond cond, bool parameter)
        {
            if (parameter)
                F |= (byte)cond;
            else
                F &= (byte)~cond;
        }

        public static byte Rl(byte v, ref byte F, bool carry)
        {
            byte result = v;
            byte bit7 = BitValue(v, 7);
            byte carryFlag = BitValue(F, 4); // index 4 is carry flag


            AlterFlag(ref F, Cond.N, false);
            AlterFlag(ref F, Cond.H, false);
            AlterFlag(ref F, Cond.C, bit7 == 1);

            result <<= 1;

            if (carry)
                result |= carryFlag;
            else
                result |= bit7;

            AlterFlag(ref F, Cond.Z, result == 0);

            return result;
        }

        public static byte Swap(byte v, ref byte F)
        {
            var hi = (v >> 4) & 0xF;
            var lo = (v >> 0) & 0xF;
            var result = (byte)(lo << 4 | hi);

            AlterFlag(ref F, Cond.Z, result == 0);
            AlterFlag(ref F, Cond.N, false);
            AlterFlag(ref F, Cond.H, false);
            AlterFlag(ref F, Cond.C, false);

            return result;
        }
    }
}
