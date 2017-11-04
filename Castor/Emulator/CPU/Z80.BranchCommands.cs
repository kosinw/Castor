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
        private void JumpRelativeIfCondition(sbyte relativeValue, bool condition)
        {
            if (condition)
            {
                JumpRelative(relativeValue);
                _cyclesToWait += 4; // action was taken
            }
        }
      
        private bool IsZero() => ((byte)StatusFlags.ZeroFlag & F) == (byte)StatusFlags.ZeroFlag;

        private void JumpRelative(sbyte relativeValue) => PC = (ushort)(PC + relativeValue);
        private void JumpRelativeIfNotZero() => JumpRelativeIfCondition((sbyte)ReadByte(), !IsZero());
        private void JumpRelativeIfZero() => JumpRelativeIfCondition((sbyte)ReadByte(), IsZero());

        private void CallSubroutine(ushort immediateValue)
        {
            SP -= 2; // stack pointer grows downward twice to fit ushort

            ushort reversedPC = PC; // get little-endian order
            _system.MMU[SP] = reversedPC.LeastSignificantByte(); // store LSB first
            _system.MMU[SP + 1] = reversedPC.MostSignificantByte(); // store MSB last

            PC = --immediateValue; // Set PC to --immediateValue (will be incremented)
        }

        private void ReturnSubroutine()
        {
            PC = Convert.ToUInt16(_system.MMU[SP + 1] << 8 | _system.MMU[SP]);
            SP += 2;
        }
    }
}
