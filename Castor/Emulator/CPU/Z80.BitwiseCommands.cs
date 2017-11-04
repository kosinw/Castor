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
        private void CheckBIT(byte index, byte value)
        {            
            bool bit = value.CheckBit((BitFlags)index); // checks if bit is set or unset

            if (bit == false) // if bit is unset then set Z flag
                F.SetBit((BitFlags)StatusFlags.ZeroFlag);
            else
                F.ClearBit((BitFlags)StatusFlags.ZeroFlag);

            F.ClearBit((BitFlags)StatusFlags.SubtractFlag); // clear N flag
            F.SetBit((BitFlags)StatusFlags.HalfCarryFlag); // set H flag
        }

        private void BitRotateLeftCarryA() => BitRotateLeftCarryR8(ref A);
        private void BitRotateLeftCarryC() => BitRotateLeftCarryR8(ref C);        

        private void BitRotateLeftCarryR8(ref byte register)
        {
            if (register == 0x80) // 0b_1000_0000
            {
                register = 0;
                F |= (byte)StatusFlags.ZeroFlag; // set zero flag
                F |= (byte)StatusFlags.CarryFlag; // set carry flag
            }
            else
            {
                bool bit7 = (register & (byte)BitFlags.Bit7) != 0;
                bool oldCarryFlag = (F & (byte)StatusFlags.CarryFlag) != 0;

                if (bit7)
                    F |= (byte)StatusFlags.CarryFlag;
                else
                    F &= (byte)~StatusFlags.CarryFlag;

                register = (byte)(register << 1 | register >> 7);

                // here shifts the old bit from the carry flag into bit0 of register
                if (oldCarryFlag)
                    register |= (byte)BitFlags.Bit0;
                else 
                    register &= (byte)~BitFlags.Bit0;

                F &= (byte)~StatusFlags.ZeroFlag; // unset zero flg
            }
        }
    }
}
