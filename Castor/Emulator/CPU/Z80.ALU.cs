using static Castor.Emulator.Utility.Bit;
using static Castor.Emulator.CPU.Registers;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        public void AluAdd(int value, bool cy)
        {
            var addend = value + (cy ? BitValue(F, Flags.C) : 0);

            var hc = ((addend & 0xF + A & 0xF) & 0x10) == 0x10;
            var c = ((addend & 0xFF + A & 0xFF) & 0x100) == 0x100;

            var result = (byte)(addend + A);

            _r[Flags.Z] = result == 0;
            _r[Flags.N] = false;
            _r[Flags.H] = hc;
            _r[Flags.C] = c;

            A = result;
        }

        public void AluAddHL(int value)
        {
            var addend = value;

            var hc = ((addend & 0xFFF + HL & 0xFFF) & 0x1000) == 0x1000;
            var c = ((addend & 0xFFFF + HL & 0xFFFF) & 0x10000) == 0x10000;

            var result = (ushort)(addend + HL);
            
            _r[Flags.N] = false;
            _r[Flags.H] = hc;
            _r[Flags.C] = c;

            HL = result;
        }

        public void AluAddSP(int value)
        {
            var addend = value;

            var hc = ((addend & 0xF + SP & 0xF) & 0x10) == 0x10;
            var c = ((addend & 0xFF + SP & 0xFF) & 0x100) == 0x100;

            var result = (ushort)(value + addend);

            _r[Flags.Z] = false;
            _r[Flags.N] = false;
            _r[Flags.H] = hc;
            _r[Flags.C] = c;

            SP = result;
        }
    }
}
