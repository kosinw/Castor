using Castor.Emulator.Memory;
using Castor.Emulator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        private void LoadIntoSPRegister(ushort value) => SP = value;
        private void LoadIntoBCRegister(ushort value) => BC = value;
        private void LoadIntoDERegister(ushort value) => DE = value;
        private void LoadIntoHLRegister(ushort value) => HL = value;        

        private void LoadAIntoHLPointerPostDecrement() { _system.MMU[HL] = A; --HL; }
        private void LoadAIntoHLPointerPostIncrement() { _system.MMU[HL] = A; ++HL; }
        private void LoadAIntoHLPointer() { _system.MMU[HL] = A; }
        private void LoadARegisterIntoCPointer() => _system.MMU[C + 0xFF00] = A;

        private void LoadARegisterIntoAddress8(byte a8) => _system.MMU[a8 + 0xFF00] = A;
        private void LoadAddress8IntoARegister(byte a8) => A = _system.MMU[a8 + 0xFF00];

        private void LoadARegisterIntoAddress16(ushort a16) => _system.MMU[a16] = A;

        private void LoadValueDEIntoA() { A = _system.MMU[DE]; }        

        private void LoadIntoARegister(byte value) => A = value;
        private void LoadIntoBRegister(byte value) => B = value;
        private void LoadIntoCRegister(byte value) => C = value;
        private void LoadIntoDRegister(byte value) => D = value;
        private void LoadIntoERegister(byte value) => E = value;
        private void LoadIntoFRegister(byte value) => F = value;
        private void LoadIntoHRegister(byte value) => H = value;
        private void LoadIntoLRegister(byte value) => L = value;

        private void PushPairOntoStack(ushort value)
        {
            SP -= 2; // decrement stack pointer twice
            
            _system.MMU[SP] = value.LeastSignificantByte(); // store LSB in memory first
            _system.MMU[SP + 1] = value.MostSignificantByte(); // store MSB in memory last
        }

        private void PopBCOffStack()
        {
            BC = Convert.ToUInt16(_system.MMU[SP + 1] << 8 | _system.MMU[SP]);

            SP += 2; // increment stack pointer twice
        }
    }
}
