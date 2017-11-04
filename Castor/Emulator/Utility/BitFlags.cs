using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    [Flags]
    public enum BitFlags : byte
    {
        None = 0,
        Bit0 = 1 << 0,
        Bit1 = 1 << 1,
        Bit2 = 1 << 2,
        Bit3 = 1 << 3,
        Bit4 = 1 << 4,
        Bit5 = 1 << 5,
        Bit6 = 1 << 6,
        Bit7 = 1 << 7
    }
}
