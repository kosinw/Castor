using Castor.Emulator.Utility;
using System;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        public enum LoadType
        {
            A,
            B,
            C,
            D,
            E,
            F,
            H,
            L,
            Imm8,
            Imm16,
            Addr8,
            Addr16,
            AddrC,
            AddrBC,
            AddrDE,
            AddrHL,
            AddrHLInc,
            AddrHLDec,
            BC,
            DE,
            HL,
            SP,
            AccessorSP
        };

        public enum StackType
        {
            BC,
            DE,
            HL,
            AF
        }

        private void PopulateLoadInstructions() // All the data for the move/load instructions
        {
            // LD Reg16,Imm16
            _operations[0x01] = RLI(LoadType.BC, LoadType.Imm16);
            _operations[0x11] = RLI(LoadType.DE, LoadType.Imm16);
            _operations[0x21] = RLI(LoadType.HL, LoadType.Imm16);
            _operations[0x31] = RLI(LoadType.SP, LoadType.Imm16);

            // LD (RegAddr),A
            _operations[0x02] = RLI(LoadType.AddrBC, LoadType.A);
            _operations[0x12] = RLI(LoadType.AddrDE, LoadType.A);
            _operations[0x22] = RLI(LoadType.AddrHLInc, LoadType.A);
            _operations[0x32] = RLI(LoadType.AddrHLDec, LoadType.A);

            // LD A,RegAddr
            _operations[0x0A] = RLI(LoadType.A, LoadType.AddrBC);
            _operations[0x1A] = RLI(LoadType.A, LoadType.AddrDE);
            _operations[0x2A] = RLI(LoadType.A, LoadType.AddrHLInc);
            _operations[0x3A] = RLI(LoadType.A, LoadType.AddrHLDec);

            // LD Reg8,Imm8
            _operations[0x06] = RLI(LoadType.B, LoadType.Imm8);
            _operations[0x16] = RLI(LoadType.D, LoadType.Imm8);
            _operations[0x26] = RLI(LoadType.H, LoadType.Imm8);
            _operations[0x36] = RLI(LoadType.AddrHL, LoadType.Imm8);

            _operations[0x0E] = RLI(LoadType.C, LoadType.Imm8);
            _operations[0x1E] = RLI(LoadType.E, LoadType.Imm8);
            _operations[0x2E] = RLI(LoadType.L, LoadType.Imm8);
            _operations[0x3E] = RLI(LoadType.A, LoadType.Imm8);

            // LDH
            _operations[0xE0] = RLI(LoadType.Addr8, LoadType.A);
            _operations[0xF0] = RLI(LoadType.A, LoadType.Addr8);

            // LD (C),A & LD A,(C)
            _operations[0xE2] = RLI(LoadType.AddrC, LoadType.A);
            _operations[0xF2] = RLI(LoadType.A, LoadType.AddrC);

            // LD (a16),A & LD A,(a16)
            _operations[0xEA] = RLI(LoadType.Addr16, LoadType.A);
            _operations[0xFA] = RLI(LoadType.A, LoadType.Addr16);

            // LD (a16), SP
            _operations[0x08] = RLI(LoadType.Addr16, LoadType.AccessorSP);

            // LD B,Reg8
            _operations[0x40] = RLI(LoadType.B, LoadType.B);
            _operations[0x41] = RLI(LoadType.B, LoadType.C);
            _operations[0x42] = RLI(LoadType.B, LoadType.D);
            _operations[0x43] = RLI(LoadType.B, LoadType.E);
            _operations[0x44] = RLI(LoadType.B, LoadType.H);
            _operations[0x45] = RLI(LoadType.B, LoadType.L);
            _operations[0x46] = RLI(LoadType.B, LoadType.AddrHL);
            _operations[0x47] = RLI(LoadType.B, LoadType.A);

            // LD C,Reg8
            _operations[0x48] = RLI(LoadType.C, LoadType.B);
            _operations[0x49] = RLI(LoadType.C, LoadType.C);
            _operations[0x4A] = RLI(LoadType.C, LoadType.D);
            _operations[0x4B] = RLI(LoadType.C, LoadType.E);
            _operations[0x4C] = RLI(LoadType.C, LoadType.H);
            _operations[0x4D] = RLI(LoadType.C, LoadType.L);
            _operations[0x4E] = RLI(LoadType.C, LoadType.AddrHL);
            _operations[0x4F] = RLI(LoadType.C, LoadType.A);

            // LD D,Reg8
            _operations[0x50] = RLI(LoadType.D, LoadType.B);
            _operations[0x51] = RLI(LoadType.D, LoadType.C);
            _operations[0x52] = RLI(LoadType.D, LoadType.D);
            _operations[0x53] = RLI(LoadType.D, LoadType.E);
            _operations[0x54] = RLI(LoadType.D, LoadType.H);
            _operations[0x55] = RLI(LoadType.D, LoadType.L);
            _operations[0x56] = RLI(LoadType.D, LoadType.AddrHL);
            _operations[0x57] = RLI(LoadType.D, LoadType.A);

            // LD E,Reg8
            _operations[0x58] = RLI(LoadType.E, LoadType.B);
            _operations[0x59] = RLI(LoadType.E, LoadType.C);
            _operations[0x5A] = RLI(LoadType.E, LoadType.D);
            _operations[0x5B] = RLI(LoadType.E, LoadType.E);
            _operations[0x5C] = RLI(LoadType.E, LoadType.H);
            _operations[0x5D] = RLI(LoadType.E, LoadType.L);
            _operations[0x5E] = RLI(LoadType.E, LoadType.AddrHL);
            _operations[0x5F] = RLI(LoadType.E, LoadType.A);

            // LD H,Reg8
            _operations[0x60] = RLI(LoadType.H, LoadType.B);
            _operations[0x61] = RLI(LoadType.H, LoadType.C);
            _operations[0x62] = RLI(LoadType.H, LoadType.D);
            _operations[0x63] = RLI(LoadType.H, LoadType.E);
            _operations[0x64] = RLI(LoadType.H, LoadType.H);
            _operations[0x65] = RLI(LoadType.H, LoadType.L);
            _operations[0x66] = RLI(LoadType.H, LoadType.AddrHL);
            _operations[0x67] = RLI(LoadType.H, LoadType.A);

            // LD L,Reg8
            _operations[0x68] = RLI(LoadType.L, LoadType.B);
            _operations[0x69] = RLI(LoadType.L, LoadType.C);
            _operations[0x6A] = RLI(LoadType.L, LoadType.D);
            _operations[0x6B] = RLI(LoadType.L, LoadType.E);
            _operations[0x6C] = RLI(LoadType.L, LoadType.H);
            _operations[0x6D] = RLI(LoadType.L, LoadType.L);
            _operations[0x6E] = RLI(LoadType.L, LoadType.AddrHL);
            _operations[0x6F] = RLI(LoadType.L, LoadType.A);

            // LD (HL),Reg8
            _operations[0x70] = RLI(LoadType.AddrHL, LoadType.B);
            _operations[0x71] = RLI(LoadType.AddrHL, LoadType.C);
            _operations[0x72] = RLI(LoadType.AddrHL, LoadType.D);
            _operations[0x73] = RLI(LoadType.AddrHL, LoadType.E);
            _operations[0x74] = RLI(LoadType.AddrHL, LoadType.H);
            _operations[0x75] = RLI(LoadType.AddrHL, LoadType.L);
            _operations[0x77] = RLI(LoadType.AddrHL, LoadType.A);

            // LD A,Reg8
            _operations[0x78] = RLI(LoadType.A, LoadType.B);
            _operations[0x79] = RLI(LoadType.A, LoadType.C);
            _operations[0x7A] = RLI(LoadType.A, LoadType.D);
            _operations[0x7B] = RLI(LoadType.A, LoadType.E);
            _operations[0x7C] = RLI(LoadType.A, LoadType.H);
            _operations[0x7D] = RLI(LoadType.A, LoadType.L);
            _operations[0x7E] = RLI(LoadType.A, LoadType.AddrHL);
            _operations[0x7F] = RLI(LoadType.A, LoadType.A);

            // LD SP,HL
            _operations[0xF9] = RLI(LoadType.AccessorSP, LoadType.HL);

            // LD HL,SP+r8
            _operations[0xF8] = () =>
            {
                sbyte si8 = (sbyte)ReadByte(PC);

                SetFlag(false, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(Bitwise.Add.CheckHalfCarry(SP, si8), StatusFlags.H);
                SetFlag(Bitwise.Add.CheckFullCarry(SP, si8), StatusFlags.C);

                HL = (ushort)(AccessorSP + si8);
            };

            // POP Reg16
            _operations[0xC1] = RPOI(StackType.BC);
            _operations[0xD1] = RPOI(StackType.DE);
            _operations[0xE1] = RPOI(StackType.HL);
            _operations[0xF1] = RPOI(StackType.AF);

            // PUSH Reg16
            _operations[0xC5] = RPUI(StackType.BC);
            _operations[0xD5] = RPUI(StackType.DE);
            _operations[0xE5] = RPUI(StackType.HL);
            _operations[0xF5] = RPUI(StackType.AF);
        }

        /// <summary>
        /// A shorthand notation to register load instructions.
        /// </summary>
        /// <param name="LHS">Left hand side of the binary operation.</param>
        /// <param name="RHS">Right hand side of the binary operation.</param>
        /// <returns>void delegate()</returns>
        Instruction RLI(LoadType LHS, LoadType RHS)
        {
            Action<int> setter = new Action<int>(f => { });

            switch (LHS)
            {
                case LoadType.A:
                    setter = v => A = (byte)v;
                    break;
                case LoadType.B:
                    setter = v => B = (byte)v;
                    break;
                case LoadType.C:
                    setter = v => C = (byte)v;
                    break;
                case LoadType.D:
                    setter = v => D = (byte)v;
                    break;
                case LoadType.E:
                    setter = v => E = (byte)v;
                    break;
                case LoadType.F:
                    setter = v => F = (byte)v;
                    break;
                case LoadType.H:
                    setter = v => H = (byte)v;
                    break;
                case LoadType.L:
                    setter = v => L = (byte)v;
                    break;
                case LoadType.Addr8:
                    setter = v => Addr8 = (byte)v;
                    break;
                case LoadType.Addr16:
                    setter = v => Addr16 = (byte)v;
                    break;
                case LoadType.AddrC:
                    setter = v => AddrC = (byte)v;
                    break;
                case LoadType.AddrBC:
                    setter = v => AddrBC = (byte)v;
                    break;
                case LoadType.AddrDE:
                    setter = v => AddrDE = (byte)v;
                    break;
                case LoadType.AddrHL:
                    setter = v => AddrHL = (byte)v;
                    break;
                case LoadType.AddrHLDec:
                    setter = v => AddrHLDec = (byte)v;
                    break;
                case LoadType.AddrHLInc:
                    setter = v => AddrHLInc = (byte)v;
                    break;
                case LoadType.BC:
                    setter = v => BC = (ushort)v;
                    break;
                case LoadType.DE:
                    setter = v => DE = (ushort)v;
                    break;
                case LoadType.HL:
                    setter = v => HL = (ushort)v;
                    break;
                case LoadType.SP:
                    setter = v => SP = (ushort)v;
                    break;
                case LoadType.AccessorSP:
                    setter = v => AccessorSP = (ushort)v;
                    break;
                default:
                    throw new Exception("Invalid left-hand load type was provided.");
            }
            switch (RHS)
            {
                case LoadType.A:
                    return () => setter(A);
                case LoadType.B:
                    return () => setter(B);
                case LoadType.C:
                    return () => setter(C);
                case LoadType.D:
                    return () => setter(D);
                case LoadType.E:
                    return () => setter(E);
                case LoadType.F:
                    return () => setter(F);
                case LoadType.H:
                    return () => setter(H);
                case LoadType.L:
                    return () => setter(L);
                case LoadType.Imm8:
                    return () => setter(ReadByte(PC));
                case LoadType.Imm16:
                    return () => setter(ReadUshort(PC));
                case LoadType.AddrC:
                    return () => setter(AddrC);
                case LoadType.Addr8:
                    return () => setter(Addr8);
                case LoadType.Addr16:
                    return () => setter(Addr16);
                case LoadType.AddrBC:
                    return () => setter(AddrBC);
                case LoadType.AddrDE:
                    return () => setter(AddrDE);
                case LoadType.AddrHL:
                    return () => setter(AddrHL);
                case LoadType.AddrHLInc:
                    return () => setter(AddrHLInc);
                case LoadType.AddrHLDec:
                    return () => setter(AddrHLDec);
                case LoadType.SP:
                    return () => setter(SP);
                case LoadType.HL:
                    return () => setter(HL);
                case LoadType.AccessorSP:
                    return () => setter(AccessorSP);
                default:
                    throw new Exception("Invalid right-hand load type was provided.");
            }
        }

        /// <summary>
        /// A shorthand notation to register push instructions.
        /// </summary>
        /// <param name="operand">The 16-bit register to push onto the stack.</param>
        /// <returns></returns>
        Instruction RPUI(StackType operand)
        {
            switch (operand)
            {
                case StackType.AF:
                    return () => PushUshort(AF);
                case StackType.BC:
                    return () => PushUshort(BC);
                case StackType.DE:
                    return () => PushUshort(DE);
                case StackType.HL:
                    return () => PushUshort(HL);
                default:
                    throw new Exception("Invalid operand supplied.");
            }
        }

        /// <summary>
        /// A shorthand notation to register pop instructions.
        /// </summary>
        /// <param name="operand">The 16-bit register to pop off of the stack.</param>
        /// <returns></returns>
        Instruction RPOI(StackType operand)
        {
            switch (operand)
            {
                case StackType.AF:
                    return () => AF = PopUshort();
                case StackType.BC:
                    return () => BC = PopUshort();
                case StackType.DE:
                    return () => DE = PopUshort();
                case StackType.HL:
                    return () => HL = PopUshort();
                default:
                    throw new Exception("Invalid operand supplied.");
            }
        }

        /// <summary>
        /// A utility function to push registers onto the stack.
        /// </summary>
        /// <param name="value"></param>
        private void PushUshort(ushort value)
        {
            SP -= 2;
            WriteUshort(SP, value);
        }

        /// <summary>
        /// A utility function to pop registers off of the stack.
        /// </summary>
        /// <returns></returns>
        private ushort PopUshort()
        {
            ushort ret = ReadUshort(SP);
            SP += 2;
            PC -= 2; // only one parameter, read ushort
            return ret;
        }
    }
}
