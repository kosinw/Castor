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
            _op[0x01] = () => { ReadByte(PC); _cyclesToWait -= 4; _halted = true; };

            // HALT
            _op[0x76] = () => { _halted = true; };

            // PREFIX CB
            _op[0xCB] = () => { _cb[ReadByte(PC)](); };

            // EI
            _op[0xFB] = () => { _setei = 2; };

            // DI
            _op[0xF3] = () => { _ime = false; _setei = 0; };
        }
    }
}
