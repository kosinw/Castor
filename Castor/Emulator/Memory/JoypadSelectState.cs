using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    [Flags]
    public enum JoypadSelectState : byte
    {
        Select    = 0b0000_0001,
        Start     = 0b0000_0010,
        A         = 0b0000_0100,
        B         = 0b0000_1000,
        Up        = 0b0001_0000,
        Left      = 0b0010_0000,
        Down      = 0b0100_0000,
        Right     = 0b1000_0000,
        None      = 0
    }
}
