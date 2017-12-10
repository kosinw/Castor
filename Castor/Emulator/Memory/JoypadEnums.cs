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
        Down        = 0b1000,
        Start       = 0b1000,

        Up          = 0b0100,
        Select      = 0b0100,
                
        Left        = 0b0010,
        B           = 0b0010,

        A           = 0b0001,
        Right       = 0b0001,

        None        = 0
    }

    [Flags]
    public enum SelectedKeys: byte
    {
        Direction  = 0b01,
        Buttons    = 0b10,
        None       = 0b00
    }
}
