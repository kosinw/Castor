using Castor.Emulator.Utility;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        public enum LoadType
        {
            A, B, C, D, E, F, H, L,
            Imm8, Imm16, Addr8, Addr16,
            _C, _BC, _DE, _HL,
            _HLI, _HLD, BC, DE,
            HL, SP, AccessorSP
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
            // ld r16,imm16
            _op[0x01] = () => { BC = ReadUshort(PC); };
            _op[0x11] = () => { DE = ReadUshort(PC); };
            _op[0x21] = () => { HL = ReadUshort(PC); };
            _op[0x31] = () => { SP = ReadUshort(PC); };

            // ld (r16),A
            _op[0x02] = () => { AddrBC = A; };
            _op[0x12] = () => { AddrDE = A; };
            _op[0x22] = () => { AddrHLI = A; };
            _op[0x32] = () => { AddrHLD = A; };

            // LD A,(r16)
            _op[0x0A] = () => { A = AddrBC; };
            _op[0x1A] = () => { A = AddrDE; };
            _op[0x2A] = () => { A = AddrHLI; };
            _op[0x3A] = () => { A = AddrHLD; };

            // LD r8,imm8
            _op[0x06] = () => { B = ReadByte(PC); };
            _op[0x16] = () => { D = ReadByte(PC); };
            _op[0x26] = () => { H = ReadByte(PC); };
            _op[0x36] = () => { AddrHL = ReadByte(PC); };

            _op[0x0E] = () => { C = ReadByte(PC); };
            _op[0x1E] = () => { E = ReadByte(PC); };
            _op[0x2E] = () => { L = ReadByte(PC); };
            _op[0x3E] = () => { A = ReadByte(PC); };

            // LDH (a8),A
            // LDH A,(a8)
            _op[0xE0] = () => { Addr8 = A; };
            _op[0xF0] = () => { A = Addr8; };

            // LD (C),A & LD A,(C)
            _op[0xE2] = RLI(LoadType._C, LoadType.A);
            _op[0xF2] = RLI(LoadType.A, LoadType._C);

            // LD (a16),A & LD A,(a16)
            _op[0xEA] = RLI(LoadType.Addr16, LoadType.A);
            _op[0xFA] = RLI(LoadType.A, LoadType.Addr16);

            // LD (a16), SP
            _op[0x08] = RLI(LoadType.Addr16, LoadType.AccessorSP);

            // LD B,Reg8
            _op[0x40] = RLI(LoadType.B, LoadType.B);
            _op[0x41] = RLI(LoadType.B, LoadType.C);
            _op[0x42] = RLI(LoadType.B, LoadType.D);
            _op[0x43] = RLI(LoadType.B, LoadType.E);
            _op[0x44] = RLI(LoadType.B, LoadType.H);
            _op[0x45] = RLI(LoadType.B, LoadType.L);
            _op[0x46] = RLI(LoadType.B, LoadType._HL);
            _op[0x47] = RLI(LoadType.B, LoadType.A);

            // LD C,Reg8
            _op[0x48] = RLI(LoadType.C, LoadType.B);
            _op[0x49] = RLI(LoadType.C, LoadType.C);
            _op[0x4A] = RLI(LoadType.C, LoadType.D);
            _op[0x4B] = RLI(LoadType.C, LoadType.E);
            _op[0x4C] = RLI(LoadType.C, LoadType.H);
            _op[0x4D] = RLI(LoadType.C, LoadType.L);
            _op[0x4E] = RLI(LoadType.C, LoadType._HL);
            _op[0x4F] = RLI(LoadType.C, LoadType.A);

            // LD D,Reg8
            _op[0x50] = RLI(LoadType.D, LoadType.B);
            _op[0x51] = RLI(LoadType.D, LoadType.C);
            _op[0x52] = RLI(LoadType.D, LoadType.D);
            _op[0x53] = RLI(LoadType.D, LoadType.E);
            _op[0x54] = RLI(LoadType.D, LoadType.H);
            _op[0x55] = RLI(LoadType.D, LoadType.L);
            _op[0x56] = RLI(LoadType.D, LoadType._HL);
            _op[0x57] = RLI(LoadType.D, LoadType.A);

            // LD E,Reg8
            _op[0x58] = RLI(LoadType.E, LoadType.B);
            _op[0x59] = RLI(LoadType.E, LoadType.C);
            _op[0x5A] = RLI(LoadType.E, LoadType.D);
            _op[0x5B] = RLI(LoadType.E, LoadType.E);
            _op[0x5C] = RLI(LoadType.E, LoadType.H);
            _op[0x5D] = RLI(LoadType.E, LoadType.L);
            _op[0x5E] = RLI(LoadType.E, LoadType._HL);
            _op[0x5F] = RLI(LoadType.E, LoadType.A);

            // LD H,Reg8
            _op[0x60] = RLI(LoadType.H, LoadType.B);
            _op[0x61] = RLI(LoadType.H, LoadType.C);
            _op[0x62] = RLI(LoadType.H, LoadType.D);
            _op[0x63] = RLI(LoadType.H, LoadType.E);
            _op[0x64] = RLI(LoadType.H, LoadType.H);
            _op[0x65] = RLI(LoadType.H, LoadType.L);
            _op[0x66] = RLI(LoadType.H, LoadType._HL);
            _op[0x67] = RLI(LoadType.H, LoadType.A);

            // LD L,Reg8
            _op[0x68] = RLI(LoadType.L, LoadType.B);
            _op[0x69] = RLI(LoadType.L, LoadType.C);
            _op[0x6A] = RLI(LoadType.L, LoadType.D);
            _op[0x6B] = RLI(LoadType.L, LoadType.E);
            _op[0x6C] = RLI(LoadType.L, LoadType.H);
            _op[0x6D] = RLI(LoadType.L, LoadType.L);
            _op[0x6E] = RLI(LoadType.L, LoadType._HL);
            _op[0x6F] = RLI(LoadType.L, LoadType.A);

            // LD (HL),Reg8
            _op[0x70] = RLI(LoadType._HL, LoadType.B);
            _op[0x71] = RLI(LoadType._HL, LoadType.C);
            _op[0x72] = RLI(LoadType._HL, LoadType.D);
            _op[0x73] = RLI(LoadType._HL, LoadType.E);
            _op[0x74] = RLI(LoadType._HL, LoadType.H);
            _op[0x75] = RLI(LoadType._HL, LoadType.L);
            _op[0x77] = RLI(LoadType._HL, LoadType.A);

            // LD A,Reg8
            _op[0x78] = RLI(LoadType.A, LoadType.B);
            _op[0x79] = RLI(LoadType.A, LoadType.C);
            _op[0x7A] = RLI(LoadType.A, LoadType.D);
            _op[0x7B] = RLI(LoadType.A, LoadType.E);
            _op[0x7C] = RLI(LoadType.A, LoadType.H);
            _op[0x7D] = RLI(LoadType.A, LoadType.L);
            _op[0x7E] = RLI(LoadType.A, LoadType._HL);
            _op[0x7F] = RLI(LoadType.A, LoadType.A);

            // LD SP,HL
            _op[0xF9] = RLI(LoadType.AccessorSP, LoadType.HL);

            // LD HL,SP+r8
            _op[0xF8] = () =>
            {
                sbyte si8 = (sbyte)ReadByte(PC);

                SetFlag(false, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(Utility.Math.Add.CheckHalfCarry(SP, si8), StatusFlags.H);
                SetFlag(Utility.Math.Add.CheckFullCarry(SP, si8), StatusFlags.C);

                HL = (ushort)(AccessorSP + si8);
            };

            // POP Reg16
            _op[0xC1] = RPOI(StackType.BC);
            _op[0xD1] = RPOI(StackType.DE);
            _op[0xE1] = RPOI(StackType.HL);
            _op[0xF1] = RPOI(StackType.AF);

            // PUSH Reg16
            _op[0xC5] = RPUI(StackType.BC);
            _op[0xD5] = RPUI(StackType.DE);
            _op[0xE5] = RPUI(StackType.HL);
            _op[0xF5] = RPUI(StackType.AF);
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
                case LoadType._C:
                    setter = v => AddrC = (byte)v;
                    break;
                case LoadType._BC:
                    setter = v => AddrBC = (byte)v;
                    break;
                case LoadType._DE:
                    setter = v => AddrDE = (byte)v;
                    break;
                case LoadType._HL:
                    setter = v => AddrHL = (byte)v;
                    break;
                case LoadType._HLD:
                    setter = v => AddrHLD = (byte)v;
                    break;
                case LoadType._HLI:
                    setter = v => AddrHLI = (byte)v;
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
                case LoadType._C:
                    return () => setter(AddrC);
                case LoadType.Addr8:
                    return () => setter(Addr8);
                case LoadType.Addr16:
                    return () => setter(Addr16);
                case LoadType._BC:
                    return () => setter(AddrBC);
                case LoadType._DE:
                    return () => setter(AddrDE);
                case LoadType._HL:
                    return () => setter(AddrHL);
                case LoadType._HLI:
                    return () => setter(AddrHLI);
                case LoadType._HLD:
                    return () => setter(AddrHLD);
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
