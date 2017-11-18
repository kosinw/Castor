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
            Si8,
            Addr8,
            Addr16,
            AddrAF,
            AddrC,
            AddrBC,
            AddrDE,
            AddrHL,
            AddrHLInc,
            AddrHLDec,
            BC,
            DE,
            HL,
            SP
        };

        public void PopulateLoadInstructions()
        {
            // LD Reg16,Imm16
            _operations[0x01] = RLI(LoadType.BC, LoadType.Imm16);
            _operations[0x11] = RLI(LoadType.DE, LoadType.Imm16);
            _operations[0x21] = RLI(LoadType.HL, LoadType.Imm16);
            _operations[0x31] = RLI(LoadType.SP, LoadType.Imm16);

            // LD RegAddr,A
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
            _operations[0x08] = RLI(LoadType.Addr16, LoadType.SP);

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
                    setter = v => B = (byte)v;
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
                    setter = v => _system.MMU[0xFF00 + ReadByte(PC)] = (byte)v;
                    break;
                case LoadType.Addr16:
                    setter = v => _system.MMU[ReadUshort(PC)] = (byte)v;
                    break;
                case LoadType.AddrC:
                    setter = v => _system.MMU[0xFF00 + C] = (byte)v;
                    break;
                case LoadType.AddrAF:
                    setter = v => _system.MMU[AF] = (byte)v;
                    break;
                case LoadType.AddrBC:
                    setter = v => _system.MMU[BC] = (byte)v;
                    break;
                case LoadType.AddrDE:
                    setter = v => _system.MMU[DE] = (byte)v;
                    break;
                case LoadType.AddrHL:
                    setter = v => _system.MMU[HL] = (byte)v;
                    break;
                case LoadType.AddrHLDec:
                    setter = v => _system.MMU[HL--] = (byte)v;
                    break;
                case LoadType.AddrHLInc:
                    setter = v => _system.MMU[HL++] = (byte)v;
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
                    return () => setter(_system.MMU[0xFF00 + C]);
                case LoadType.Addr8:
                    return () => setter(_system.MMU[0xFF00 + ReadByte(PC)]);
                case LoadType.Addr16:
                    return () => setter(_system.MMU[ReadUshort(PC)]);
                case LoadType.AddrBC:
                    return () => setter(_system.MMU[BC]);
                case LoadType.AddrDE:
                    return () => setter(_system.MMU[DE]);
                case LoadType.AddrHL:
                    return () => setter(_system.MMU[HL]);
                case LoadType.AddrHLInc:
                    return () => setter(_system.MMU[HL++]);
                case LoadType.AddrHLDec:
                    return () => setter(_system.MMU[HL--]);
                default:
                    throw new Exception("Invalid right-hand load type was provided.");
            }
        }
    }
}
