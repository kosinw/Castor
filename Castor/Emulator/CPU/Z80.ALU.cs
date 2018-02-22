using static Castor.Emulator.Utility.Bit;
using static Castor.Emulator.CPU.Registers;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        byte AluAdd(int value, bool withCarry)
        {
            var addend = value + (withCarry ? BitValue(F, Flags.C) : 0);

            var hc = ((addend & 0xF + A & 0xF) & 0x10) == 0x10;
            var c = ((addend & 0xFF + A & 0xFF) & 0x100) == 0x100;

            var result = (byte)(addend + A);

            _r[Flags.Z] = result == 0;
            _r[Flags.N] = false;
            _r[Flags.H] = hc;
            _r[Flags.C] = c;

            return result;
        }

        ushort AluAddHL(int value)
        {
            var addend = value;

            var hc = ((addend & 0xFFF + HL & 0xFFF) & 0x1000) == 0x1000;
            var c = ((addend & 0xFFFF + HL & 0xFFFF) & 0x10000) == 0x10000;

            var result = (ushort)(addend + HL);
            
            _r[Flags.N] = false;
            _r[Flags.H] = hc;
            _r[Flags.C] = c;

            return result;
        }

        ushort AluAddSP(int value)
        {
            var addend = value;

            var hc = ((addend & 0xF + SP & 0xF) & 0x10) == 0x10;
            var c = ((addend & 0xFF + SP & 0xFF) & 0x100) == 0x100;

            var result = (ushort)(value + addend);

            _r[Flags.Z] = false;
            _r[Flags.N] = false;
            _r[Flags.H] = hc;
            _r[Flags.C] = c;

            return result;
        }

        byte AluAnd(int value)
        {
            var operand = value;

            var result = (byte)(value & A);

            _r[Flags.Z] = result == 0;
            _r[Flags.N] = false;
            _r[Flags.H] = true;
            _r[Flags.C] = false;

            return result;
        }

        byte AluSub(int value, bool cy)
        {
            var operand = value + (cy ? BitValue(F, Flags.C) : 0);

            var result = (byte)(A - operand);
            var h = ((A & 0xF) - (operand & 0xF)) < 0;
            var c = (operand & 0xFF) > (A & 0xFF);            

            _r[Flags.Z] = result == 0;
            _r[Flags.N] = true;
            _r[Flags.H] = h;
            _r[Flags.C] = c;

            return result;
        }

        byte AluOr(int value)
        {
            var operand = value;

            var result = (byte)(operand | A);

            _r[Flags.Z] = result == 0;
            _r[Flags.N] = false;
            _r[Flags.H] = false;
            _r[Flags.C] = false;

            return result;
        }

        byte AluRl(int value, bool withCarry)
        {
            var operand = (byte)value;

            var shiftedBit = BitValue(operand, 7);
            var result = (byte)(operand << 1);

            result |= withCarry ? BitValue(F, Flags.C) : shiftedBit;

            _r[Flags.Z] = result == 0;
            _r[Flags.N] = false;
            _r[Flags.H] = false;
            _r[Flags.C] = shiftedBit == 1;

            return result;
        }

        byte AluRr(int value, bool withCarry)
        {
            var operand = (byte)value;

            var shiftedBit = BitValue(operand, 0);
            var result = (byte)(operand >> 1);

            result |= (byte)((withCarry ? BitValue(F, Flags.C) : shiftedBit) << 7);

            _r[Flags.Z] = result == 0;
            _r[Flags.N] = false;
            _r[Flags.H] = false;
            _r[Flags.C] = shiftedBit == 1;

            return result;
        }

        byte AluXor(int value)
        {
            var operand = value;

            var result = (byte)(operand ^ A);

            _r[Flags.Z] = result == 0;
            _r[Flags.N] = false;
            _r[Flags.H] = false;
            _r[Flags.C] = false;

            return result;
        }
    }
}
