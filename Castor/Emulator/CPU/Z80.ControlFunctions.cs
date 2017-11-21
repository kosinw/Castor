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
            _operations[0x00] = () => { };

            // STOP 0
            _operations[0x01] = () => { ReadByte(PC); _cyclesToWait -= 4; _halted = true; };

            // HALT
            _operations[0x76] = () => { _halted = true; };

            // PREFIX CB
            _operations[0xCB] = () => { _extendedOperations[ReadByte(PC)](); };

            // EI
            _operations[0xFB] = () => { _eiflag = 2; };

            // DI
            _operations[0xF3] = () => { _ime = false; _eiflag = 0; };
        }
    }
}
