using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        private void PopulateControlFunctions()
        {
            // NOP
            _op[0x00] = () => { };

            // STOP 0
            // TODO: More accurately emulate the stop command by turning off lcd, timers, + sound systems.
            _op[0x01] = () => { ReadByte(PC); _waitcycles -= 4; _halted = true; };

            // HALT
            _op[0x76] = () => { _halted = true; };

            // PREFIX CB
            _op[0xCB] = () => { _cb[ReadByte(PC)](); };

            // EI
            _op[0xFB] = () => { _setime = InterruptToggle.EnableInterruptSoon; };

            // DI
            _op[0xF3] = () => { _setime = InterruptToggle.DisableInterrupt; };
        }
    }
}
