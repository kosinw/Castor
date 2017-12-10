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
        public static class Sub
        {
            public static byte Dec(byte value, ref byte F)
            {
                var result = (byte)(value - 1);

                Bit.AlterFlag(ref F, Cond.Z, result == 0);
                Bit.AlterFlag(ref F, Cond.N, true);
                Bit.AlterFlag(ref F, Cond.H, result % 16 == 15);

                return result;
            }

            public static byte Subt(byte value, ref Registers R)
            {
                var result = (byte)(R.A - value);

                Bit.AlterFlag(ref R.F, Cond.Z, result == 0);
                Bit.AlterFlag(ref R.F, Cond.N, true);
                Bit.AlterFlag(ref R.F, Cond.H, result % 16 == 15);
                Bit.AlterFlag(ref R.F, Cond.C, (result & 0xFF) > (value & 0xFF));

                return result;
            }
        }

        public static class Add
        {
            public static byte Add8(byte value, ref Registers R)
            {
                var result = (byte)(R.A + value);

                Bit.AlterFlag(ref R.F, Cond.Z, result == 0);
                Bit.AlterFlag(ref R.F, Cond.N, false);
                Bit.AlterFlag(ref R.F, Cond.H, result % 16 == 0);
                Bit.AlterFlag(ref R.F, Cond.C, (result & 0xFF) < (value & 0xFF));

                return result;
            }

            public static ushort Add16(ushort value, ref Registers R)
            {
                var result = (ushort)(R.HL + value);

                Bit.AlterFlag(ref R.F, Cond.N, false);
                Bit.AlterFlag(ref R.F, Cond.H, result % 256 == 0);
                Bit.AlterFlag(ref R.F, Cond.C, (result & 0xFFFF) < (value & 0xFFFF));

                return result;
            }

            public static byte Inc(byte value, ref byte F)
            {
                var result = (byte)(value + 1);

                Bit.AlterFlag(ref F, Cond.Z, result == 0);
                Bit.AlterFlag(ref F, Cond.N, false);
                Bit.AlterFlag(ref F, Cond.H, result % 16 == 0);

                return result;
            }
        }

        public static byte Or(byte in8, ref Registers R)
        {
            var result = (byte)(R.A | in8);

            Bit.AlterFlag(ref R.F, Cond.Z, result == 0);
            Bit.AlterFlag(ref R.F, Cond.N, false);
            Bit.AlterFlag(ref R.F, Cond.H, false);
            Bit.AlterFlag(ref R.F, Cond.C, false);

            return result;
        }

        public static byte And(byte in8, ref Registers R)
        {
            var result = (byte)(R.A & in8);

            Bit.AlterFlag(ref R.F, Cond.Z, result == 0);
            Bit.AlterFlag(ref R.F, Cond.N, false);
            Bit.AlterFlag(ref R.F, Cond.H, true);
            Bit.AlterFlag(ref R.F, Cond.C, false);

            return result;
        }
    }
}
