using System;

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
            _op[0xA0] = RAI(() => B, 0);
            _op[0xA1] = RAI(() => C, 0);
            _op[0xA2] = RAI(() => D, 0);
            _op[0xA3] = RAI(() => E, 0);
            _op[0xA4] = RAI(() => H, 0);
            _op[0xA5] = RAI(() => L, 0);
            _op[0xA6] = RAI(() => AddrHL, 0);
            _op[0xA7] = RAI(() => A, 0);
            _op[0xE6] = RAI(() => ReadByte(PC), 0);

            // INC instructions
            _op[0x03] = RII(() => BC++, false, 4);
            _op[0x13] = RII(() => DE++, false, 4);
            _op[0x23] = RII(() => HL++, false, 4);
            _op[0x33] = RII(() => SP++, false, 4);

            _op[0x04] = RII(() => B++, true, 0);
            _op[0x14] = RII(() => D++, true, 0);
            _op[0x24] = RII(() => H++, true, 0);
            _op[0x34] = RII(() => AddrHL++, true, 0);

            _op[0x0C] = RII(() => C++, true, 0);
            _op[0x1C] = RII(() => E++, true, 0);
            _op[0x2C] = RII(() => L++, true, 0);
            _op[0x3C] = RII(() => A++, true, 0);

            // CPL instruction
            _op[0x2F] = () =>
            {
                A = (byte)~A;
                SetFlag(true, StatusFlags.N);
                SetFlag(true, StatusFlags.H);
            };
        }

        /// <summary>
        /// A shorthand notation to register AND Instructions.
        /// </summary>
        /// <param name="getter">Return an integral type to be ANDed with.</param>
        /// <param name="extraCycles">Add extra cycles</param>
        /// <returns></returns>
        Instruction RAI(Func<int> getter, int extraCycles)
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
        /// A shorthand notation to register INC instructions.
        /// </summary>
        /// <param name="setter">A func that returns the incremented value.</param>
        /// <param name="setFlags">A check that states if should check flags.</param>
        /// <param name="extraCycles">Add extra cycles if needed.</param>
        /// <returns></returns>
        Instruction RII(Func<int> setter, bool setFlags, int extraCycles)
        {
            return delegate
            {
                int result = setter();

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
