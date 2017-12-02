using System;
using Math = Castor.Emulator.Utility.Math;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {

        /// <summary>
        /// Populate all AND, XOR, OR instructions.
        /// </summary>
        private void PopulateALUInstructions()
        {
            // AND instructions
            _op[0xA0] = RANI(() => B, 0);
            _op[0xA1] = RANI(() => C, 0);
            _op[0xA2] = RANI(() => D, 0);
            _op[0xA3] = RANI(() => E, 0);
            _op[0xA4] = RANI(() => H, 0);
            _op[0xA5] = RANI(() => L, 0);
            _op[0xA6] = RANI(() => _system.MMU[HL], 4);
            _op[0xA7] = RANI(() => A, 0);
            _op[0xE6] = RANI(() => ReadByte(PC), 0);

            // INC instructions
            _op[0x03] = RII(() => ++BC, false, 4);
            _op[0x13] = RII(() => ++DE, false, 4);
            _op[0x23] = RII(() => ++HL, false, 4);
            _op[0x33] = RII(() => ++SP, false, 4);

            _op[0x04] = RII(() => ++B, true, 0);
            _op[0x14] = RII(() => ++D, true, 0);
            _op[0x24] = RII(() => ++H, true, 0);
            _op[0x34] = RII(() => ++_system.MMU[HL], true, 4);

            _op[0x0C] = RII(() => ++C, true, 0);
            _op[0x1C] = RII(() => ++E, true, 0);
            _op[0x2C] = RII(() => ++L, true, 0);
            _op[0x3C] = RII(() => ++A, true, 0);

            // CPL instruction
            _op[0x2F] = () =>
            {
                A = (byte)~A;
                SetFlag(true, StatusFlags.N);
                SetFlag(true, StatusFlags.H);
            };

            // XOR instructions
            _op[0xA8] = RXI(() => A = Math.Xor(A, B), 0);
            _op[0xA9] = RXI(() => A = Math.Xor(A, C), 0);
            _op[0xAA] = RXI(() => A = Math.Xor(A, D), 0);
            _op[0xAB] = RXI(() => A = Math.Xor(A, E), 0);
            _op[0xAC] = RXI(() => A = Math.Xor(A, H), 0);
            _op[0xAD] = RXI(() => A = Math.Xor(A, L), 0);
            _op[0xAE] = RXI(() => A = Math.Xor(A, _system.MMU[HL]), 4);
            _op[0xAF] = RXI(() => A = Math.Xor(A, A), 0);
            _op[0xEE] = RXI(() => A = Math.Xor(A, ReadByte(PC)), 0);

            // ADD instructions
            _op[0x80] = RADI(() => B, false, 0);
            _op[0x81] = RADI(() => C, false, 0);
            _op[0x82] = RADI(() => D, false, 0);
            _op[0x83] = RADI(() => E, false, 0);
            _op[0x84] = RADI(() => H, false, 0);
            _op[0x85] = RADI(() => L, false, 0);
            _op[0x86] = RADI(() => _system.MMU[HL], false, 4);
            _op[0x87] = RADI(() => A, false, 0);
            _op[0xC6] = RADI(() => ReadByte(PC), false, 0);

            // ADD HL instructions
            _op[0x09] = RAHLI(() => HL += BC);
            _op[0x19] = RAHLI(() => HL += DE);
            _op[0x29] = RAHLI(() => HL += HL);
            _op[0x39] = RAHLI(() => HL += SP);
        }

        /// <summary>
        /// A shorthand notation to register AND Instructions.
        /// </summary>
        /// <param name="getter">Return an integral type to be ANDed with.</param>
        /// <param name="extraCycles">Add extra cycles</param>
        /// <returns></returns>
        Instruction RANI(Func<int> getter, int extraCycles)
        {
            return delegate
            {
                A = (byte)(A & getter.Invoke());

                SetFlag(A == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(true, StatusFlags.H);
                SetFlag(false, StatusFlags.C);

                AddWaitCycles(extraCycles);
            };
        }

        /// <summary>
        /// A shorthand notation to register ADD instructions.
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="withCarry"></param>
        /// <param name="extraCycles"></param>
        /// <returns></returns>
        Instruction RADI(Func<byte> operandFn, bool withCarry, int extraCycles)
        {
            return delegate
            {
                byte operand = operandFn.Invoke();

                if (!withCarry) // ADD
                {
                    SetFlag(Math.Add.CheckZero(A, operand), StatusFlags.Z);
                    SetFlag(false, StatusFlags.N);
                    SetFlag(Math.Add.CheckHalfCarry(A, operand), StatusFlags.H);
                    SetFlag(Math.Add.CheckFullCarry(A, operand), StatusFlags.C);

                    A += operand;
                }

                else // ADC
                {
                    int cy = (F >> 4) & 1;

                    SetFlag(Math.Add.CheckZero(A, operand, cy), StatusFlags.Z);
                    SetFlag(false, StatusFlags.N);
                    SetFlag(Math.Add.CheckHalfCarry(A, operand, cy), StatusFlags.H);
                    SetFlag(Math.Add.CheckFullCarry(A, operand, cy), StatusFlags.C);
                }

                AddWaitCycles(extraCycles);
            };
        }

        /// <summary>
        /// A shorthand notation to register ADD HL instructions.
        /// </summary>
        /// <param name="operandFn"></param>
        /// <returns></returns>
        Instruction RAHLI(Func<ushort> operandFn)
        {
            return delegate
            {
                ushort operand = operandFn.Invoke();
                
                SetFlag(false, StatusFlags.N);
                SetFlag(Math.Add.CheckHalfCarry(HL, operand), StatusFlags.H);
                SetFlag(Math.Add.CheckFullCarry(HL, operand), StatusFlags.C);

                AddWaitCycles(4);
            };
        }

        /// <summary>
        /// A shorthand notation to register XOR instructions.
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="extraCycles"></param>
        /// <returns></returns>
        Instruction RXI(Func<int> fn, int extraCycles)
        {
            return delegate
            {
                var result = fn.Invoke();

                F = 0;
                SetFlag(result == 0, StatusFlags.Z);

                AddWaitCycles(extraCycles);
            };
        }

        /// <summary>
        /// A shorthand notation to register INC instructions.
        /// </summary>
        /// <param name="fn">A func that returns the incremented value.</param>
        /// <param name="setFlags">A check that states if should check flags.</param>
        /// <param name="extraCycles">Add extra cycles if needed.</param>
        /// <returns></returns>
        Instruction RII(Func<int> fn, bool setFlags, int extraCycles)
        {
            return delegate
            {
                int result = fn.Invoke();

                AddWaitCycles(extraCycles);

                if (setFlags) // if a byte, set flag
                {
                    SetFlag(result == 0, StatusFlags.Z);
                    SetFlag(false, StatusFlags.N);
                    SetFlag(result % 16 == 0, StatusFlags.H);
                }
            };
        }
    }
}
