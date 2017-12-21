using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    [Flags]
    public enum InterruptFlags : byte
    {
        VBL = 1 << 0,
        STAT = 1 << 1,
        Timer = 1 << 2,
        Serial = 1 << 3,
        Joypad = 1 << 4
    }
}
