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
        public static byte MSB(this ushort d16)
        {
            return Convert.ToByte((d16 >> 8) & 0xFF);
        }

        public static byte LSB(this ushort d16)
        {
            return Convert.ToByte((d16 >> 0) & 0xFF);
        }

        public static byte Value(byte d8, uint index, ref byte F)
        {
            var result = (byte)(d8 >> (int)index & 1);

            AlterFlag(ref F, Cond.Z, result == 0);
            AlterFlag(ref F, Cond.N, false);
            AlterFlag(ref F, Cond.H, true);

            return result;
        }

        private static byte BitValue(byte d8, int index)
        {
            return (byte)(d8 >> index & 1);
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
            {
                if (carryFlag != 0)
                    result += 1;
                
            }
            else
            {
                if (bit7 != 0)
                    result += 1;
            }

            AlterFlag(ref F, Cond.Z, result == 0);

            return result;
        }
    }
}
