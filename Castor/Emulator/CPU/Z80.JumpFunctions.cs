using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        private void PopulateJumpFunctions()
        {
            // JR Nx,r8
            _op[0x20] = RJI(() => !CheckFlag(StatusFlags.Z), false,
                () => (sbyte)ReadByte(PC), 0, 4);
            _op[0x30] = RJI(() => !CheckFlag(StatusFlags.C), false,
                () => (sbyte)ReadByte(PC), 0, 4);

            // JR x,r8
            _op[0x18] = RJI(() => true, false,
                () => (sbyte)ReadByte(PC), 0, 4);
            _op[0x28] = RJI(() => CheckFlag(StatusFlags.Z), false,
                () => (sbyte)ReadByte(PC), 0, 4);
            _op[0x38] = RJI(() => CheckFlag(StatusFlags.C), false,
                () => (sbyte)ReadByte(PC), 0, 4);

            // JP Nx,a16
            _op[0xC2] = RJI(() => !CheckFlag(StatusFlags.Z), true,
                () => ReadUshort(PC), 0, 4);
            _op[0xD2] = RJI(() => !CheckFlag(StatusFlags.C), true,
                () => ReadUshort(PC), 0, 4);

            // JP x,a16
            _op[0xC3] = RJI(() => true, true, 
                () => ReadUshort(PC), 0, 4);
            _op[0xCA] = RJI(() => CheckFlag(StatusFlags.Z), true,
                () => ReadUshort(PC), 0, 4);
            _op[0xDA] = RJI(() => CheckFlag(StatusFlags.C), true,
                () => ReadUshort(PC), 0, 4);
            _op[0xE9] = RJI(() => true, true,
                () => HL, 0, 0);

            // CALL
            _op[0xC4] = RCI(() => !CheckFlag(StatusFlags.Z), () => ReadUshort(PC), 0);
            _op[0xD4] = RCI(() => !CheckFlag(StatusFlags.C), () => ReadUshort(PC), 0);
            _op[0xCC] = RCI(() => CheckFlag(StatusFlags.Z), () => ReadUshort(PC), 0);
            _op[0xDC] = RCI(() => CheckFlag(StatusFlags.C), () => ReadUshort(PC), 0);
            _op[0xCD] = RCI(() => true, () => ReadUshort(PC), 0);

            // RST
            _op[0xC7] = RCI(() => true, () => 0x00, 0);
            _op[0xD7] = RCI(() => true, () => 0x10, 0);
            _op[0xE7] = RCI(() => true, () => 0x20, 0);
            _op[0xF7] = RCI(() => true, () => 0x30, 0);
            _op[0xCF] = RCI(() => true, () => 0x08, 0);
            _op[0xDF] = RCI(() => true, () => 0x18, 0);
            _op[0xEF] = RCI(() => true, () => 0x28, 0);
            _op[0xFF] = RCI(() => true, () => 0x38, 0);

            // RET
            _op[0xC0] = RRI(() => !CheckFlag(StatusFlags.Z), 4, false);
            _op[0xD0] = RRI(() => !CheckFlag(StatusFlags.C), 4, false);
            _op[0xC8] = RRI(() => CheckFlag(StatusFlags.Z), 4, false);
            _op[0xD8] = RRI(() => CheckFlag(StatusFlags.C), 4, false);
            _op[0xC9] = RRI(() => true, 4, false);
            _op[0xD9] = RRI(() => true, 4, true);
        }

        /// <summary>
        /// A shorthand notation to register jump instructions. 
        /// </summary>
        /// <param name="condition">The condition needed to be met.</param>
        /// <param name="isAbsolute">If set then do an abs jump, otherwise to a rel jump.</param>
        /// <param name="address">Address to add to PC or set to PC.</param>
        /// <param name="extraCycles">Extra cycles if needed.</param>
        /// <param name="actionTakenCycles">If action is taken, increment cyclesToWait by this much.</param>
        /// <returns></returns>
        Instruction RJI(Func<bool> condition, bool isAbsolute,
            Func<int> address, int extraCycles,
            int actionTakenCycles)
        {
            return delegate
            {
                int addressInvoked = address.Invoke();

                if (condition.Invoke())
                {                    
                    if (isAbsolute)
                        PC = (ushort)addressInvoked;
                    else
                        PC = (ushort)(PC + addressInvoked);

                    _cyclesToWait += actionTakenCycles;                    
                }

                _cyclesToWait += extraCycles;
            };
        }

        /// <summary>
        /// A shorthand notation to register call instructions.
        /// </summary>
        /// <param name="condition">The condition needed to be met.</param>
        /// <param name="address">If set then do an abs jump, otherwise to a rel jump.</param>
        /// <returns></returns>
        Instruction RCI(Func<bool> condition, Func<int> address, int extraCycles)
        {
            return delegate
            {
                int addressInvoked = address.Invoke();
                if (condition.Invoke())
                {
                    SP -= 2;
                    WriteUshort(SP, PC); // +12 cycles
                    PC = (ushort)addressInvoked;
                }

                _cyclesToWait += extraCycles;
            };
        }

        /// <summary>
        /// A shorthand notation to register return instructions.
        /// </summary>
        Instruction RRI(Func<bool> condition, int extraCycles, bool setIme)
        {
            return delegate
            {
                if (condition.Invoke())
                {
                    PC = PopUshort();
                    _cyclesToWait += 4;

                    if (setIme)
                        _ime = true;
                }

                _cyclesToWait += extraCycles;
            };
        }

        /// <summary>
        /// A utility function that does a relative jump.
        /// </summary>
        /// <param name="relativeValue"></param>
        private void JumpRelative(sbyte relativeValue)
        {
            PC = (ushort)((PC + relativeValue) & 0xFFFF);
            //_cyclesToWait += 4;
        }

        /// <summary>
        /// A utility function that does an absolute jump.
        /// </summary>
        /// <param name="addr"></param>
        private void JumpAbsolute(ushort addr)
        {
            PC = addr;
            //_cyclesToWait += 4;
        }
    }
}
