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
        C = 1 << 5,
        H = 1 << 4
    }
}
