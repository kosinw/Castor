using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        private void XORNWithA(byte n)
        {
            A ^= (byte)(n & 0xF);

            if (A == 0)
                F |= (byte)StatusFlags.ZeroFlag;
            else
                F &= (byte)~StatusFlags.ZeroFlag;

            F &= (byte)~StatusFlags.SubtractFlag;            
            F &= (byte)~StatusFlags.HalfCarryFlag;
            F &= (byte)~StatusFlags.CarryFlag;
        }

        private void XORAWithA() => XORNWithA(A);
        private void XORBWithA() => XORNWithA(B);
        private void XORCWithA() => XORNWithA(C);
        private void XORDWithA() => XORNWithA(D);
        private void XOREWithA() => XORNWithA(E);
        private void XORHWithA() => XORNWithA(H);
        private void XORLWithA() => XORNWithA(L);
        private void XORAWithHLPointer() => XORNWithA(_system.MMU[HL]);
        private void XORImmediateWithA(byte imm) => XORNWithA(imm);
        private void DecrementR8(ref byte register)
        {
            --register;

            if (register == 0)
                F |= (byte)StatusFlags.ZeroFlag;
            else
                F &= (byte)~StatusFlags.ZeroFlag;

            F |= (byte)StatusFlags.SubtractFlag;

            if (register % 16 == 0) // if half borrow
                F |= (byte)StatusFlags.HalfCarryFlag;
            else
                F &= (byte)~StatusFlags.HalfCarryFlag;
            
        }
        private void IncrementREG(ref byte register)
        {
            ++register;

            if (register == 0)
                F |= (byte)StatusFlags.ZeroFlag;
            else
                F &= (byte)~StatusFlags.ZeroFlag;

            F &= (byte)~StatusFlags.SubtractFlag; // always unset subtract flag

            if (register % 16 == 0) // if half carry
                F |= (byte)StatusFlags.HalfCarryFlag;
            else
                F &= (byte)~StatusFlags.HalfCarryFlag;
        }

        private void IncrementHL() => ++HL;
        private void IncrementDE() => ++DE;        
        private void IncrementSP() => ++SP;
        private void IncrementBC() => ++BC;

        private void CompareWithA(byte value)
        {
            if (value == A)
                F |= (byte)StatusFlags.ZeroFlag;
            else
                F &= (byte)~StatusFlags.ZeroFlag;

            F |= (byte)StatusFlags.SubtractFlag;

            if (value >= A)
                F &= (byte)~StatusFlags.CarryFlag;
            else
                F |= (byte)StatusFlags.CarryFlag;

            if ((A - value) % 16 == 0)
                F |= (byte)StatusFlags.HalfCarryFlag;
            else
                F &= (byte)~StatusFlags.HalfCarryFlag;                    
        }
    }
}
