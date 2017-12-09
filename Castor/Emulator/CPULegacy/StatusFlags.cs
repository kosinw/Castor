using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPULegacy
{
    [Flags]
    public enum StatusFlags : byte
    {
        None = 0,
        Z = (1 << 7),
        N = (1 << 6),
        H = (1 << 5),
        C = (1 << 4)
    }
}
