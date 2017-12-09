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
        public static byte MostSignificantByte(this ushort d16)
        {
            return Convert.ToByte((d16 >> 8) & 0xFF);
        }

        public static byte LeastSignificantByte(this ushort d16)
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

        public static void AlterFlag(ref byte F, Cond cond, bool parameter)
        {
            if (parameter)
                F |= (byte)cond;
            else
                F &= (byte)~cond;
        }
    }
}
