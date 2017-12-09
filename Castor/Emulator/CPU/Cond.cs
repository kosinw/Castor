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
        
        NZ = 1 << 7,
        NC = 1 << 4
    }

    public static class CondImpl
    {
        public static byte Test(this Cond c, bool condition)
        {
            return condition ? (byte)c : (byte)0;
        }
    }
}
