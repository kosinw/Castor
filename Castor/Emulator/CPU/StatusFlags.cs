using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    [Flags]
    public enum StatusFlags : byte
    {
        None = 0,
        ZeroFlag = (1 << 7),
        SubtractFlag = (1 << 6),
        HalfCarryFlag = (1 << 5),
        CarryFlag = (1 << 4)
    }
}
