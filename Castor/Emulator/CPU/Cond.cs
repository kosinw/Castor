using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    [Flags]
    public enum Cond : byte
    {
        Z = 1 << 7,
        N = 1 << 6,
        H = 1 << 5,
        C = 1 << 4,

        NZ,
        NC
    }

    public static class CondImpl
    {
        public static byte Test(this Cond c, bool condition)
        {
            return condition ? (byte)c : (byte)0;
        }

        public static bool FlagSet(this Cond c, byte F)
        {
            bool ret = false;
            var newCond = c;

            if (c == Cond.NZ)
            {
                newCond = Cond.Z;
            }

            if (c == Cond.NC)
            {
                newCond = Cond.C;
            }

            ret = ((byte)newCond & F) == (byte)newCond;

            if (c == Cond.NZ || c == Cond.NC)
            {
                ret = !ret;
            }

            return ret;
        }
    }
}