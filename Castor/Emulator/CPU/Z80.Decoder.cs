using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        public void Decode(byte op)
        {
            int z = (op & 0b00_000_111) >> 0;
            int y = (op & 0b00_111_000) >> 3;
            int x = (op & 0b11_000_000) >> 6;
            int p = (op & 0b00_110_000) >> 4;
            int q = (op & 0b00_001_000) >> 3;

            if (x == 0)
            {
                switch (z)
                {
                    #region Relative Jumps and Assorted Operations
                    case 0:
                        {
                            switch (y)
                            {
                                case 0: Nop(); return;
                                case 1: Load(ADR, NW, RP, sp); return;
                                case 2: Stop(); return;
                                case 3: JumpRelative(); return;
                                case var r when r >= 4 && r <= 7: JumpRelative(y - 4); return;
                            }

                            break;
                        }
                    #endregion

                    #region 16-bit Load Immediate/Add
                    case 1:
                        {
                            switch (q)
                            {
                                case 0: Load16(RP, p); return;
                                case 1: AddHL(p); return;
                            }

                            break;
                        }
                    #endregion

                    #region Indirect Loading
                    case 2:
                        {
                            switch ((q << 2 | p))
                            {
                                case 0: Load(ADR, BC, R, a); return;
                                case 1: Load(ADR, DE, R, a); return;
                                case 2: Load(ADR, HLI, R, a); return;
                                case 3: Load(ADR, HLD, R, a); return;

                                case 4: Load(R, a, ADR, BC); return;
                                case 5: Load(R, a, ADR, DE); return;
                                case 6: Load(R, a, ADR, HLI); return;
                                case 7: Load(R, a, ADR, HLD); return;
                            }

                            break;
                        }
                    #endregion

                    #region 16-bit Increment/Decrement
                    case 3:
                        {
                            switch (q)
                            {
                                case 0: Increment(RP, p); return;
                                case 1: Decrement(RP, p); return;
                            }
                            break;
                        }
                    #endregion

                    #region 8-bit Increment
                    case 4: Increment(R, y); return;
                    #endregion

                    #region 8-bit Decrement
                    case 5: Decrement(R, y); return;
                    #endregion

                    #region 8-bit Load Immediate
                    case 6: Load8(R, y); return;
                    #endregion

                    #region Assorted Operations on Accumulator/Flags
                    case 7:
                        {
                            switch (y)
                            {
                                case 0: RotateLeftAkC(); return;
                                case 1: RotateRightAkC(); return;
                                case 2: RotateLeftAkk(); return;
                                case 3: RotateRightAkC(); return;
                                case 4: DecimalAdjustAkk(); return;
                                case 5: Complement(); return;
                                case 6: SetCarryFlag(); return;
                                case 7: XorCarryFlag(); return;
                            }

                            break;
                        }
                        #endregion
                }
            }

            else if (x == 1)
            {
                #region Halt 
                if (z == 6)
                {
                    Halt();
                    return;
                }
                #endregion

                #region 8-bit Loading
                else
                {
                    Load(R, y, R, z);
                    return;
                }
                #endregion
            }

            else if (x == 2)
            {
                #region Operate on accumulator and register/memory location
                switch (y)
                {
                    case 0: Add(z); return;
                    case 1: Adc(z); return;
                    case 2: Sub(z); return;
                    case 3: Sbc(z); return;
                    case 4: And(z); return;
                    case 5: Xor(z); return;
                    case 6: Or(z); return;
                    case 7: Cp(z); return;
                }
                #endregion
            }

            else if (x == 3)
            {
                switch (z)
                {
                    #region Assorted Instructions
                    case 0:
                        {
                            switch (y)
                            {
                                case var r when r >= 0 && r <= 3: Ret(y); return;
                                case 4: Load(ADR, NB + 0xFF00, R, a); return;
                                case 5: AddSP(); return;
                                case 6: Load(R, a, ADR, NB + 0xFF00); return;
                                case 7: LoadHL(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Pop and Various Instructions
                    case 1:
                        {
                            switch ((q << 2 | p))
                            {
                                case 0: Pop(y); return;
                                case 4: Ret(); return;
                                case 5: Reti(); return;
                                case 6: JumpHL(); return;
                                case 7: LoadSP(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Conditional Jumps + Load A from Mem-mapped Region
                    case 2:
                        {
                            switch (y)
                            {
                                case var r when r >= 0 && r <= 3: JumpAbsolute(y); return;
                                case 4: Load(ADR, C + 0xFF00, R, a); return;
                                case 5: Load(ADR, NW, R, a); return;
                                case 6: Load(R, a, ADR, C + 0xFF00); return;
                                case 7: Load(R, a, ADR, NW); return;
                            }

                            break;
                        }
                    #endregion

                    #region More Assorted Instructions
                    case 3:
                        {
                            switch (y)
                            {
                                case 0: JumpAbsolute(); return;
                                case 1: DecodeCB(); return;
                                case 6: Di(); return;
                                case 7: Ei(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Conditional Call
                    case 4:
                        {
                            if (y >= 0 && y <= 3)
                            {
                                Call(y);
                                return;
                            }

                            break;
                        }
                    #endregion

                    #region Push and Call
                    case 5:
                        {
                            switch ((q << 2 | p))
                            {
                                case 0: Push(y); return;
                                case 4: Call(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Operate on Akk and Imm Operand
                    case 6:
                        {
                            switch (y)
                            {
                                case 0: Add(); return;
                                case 1: Adc(); return;
                                case 2: Sub(); return;
                                case 3: Sbc(); return;
                                case 4: And(); return;
                                case 5: Xor(); return;
                                case 6: Or(); return;
                                case 7: Cp(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Restart
                    case 7: Rst((ushort)(y * 8)); return;
                    #endregion
                }
            }

            throw Unimplemented(op);

            #region Deprecated Decode Logic
            //switch (op)
            //{
            //    #region 8-bit loads
            //    case 0x7F: Load(ref A, A); break;
            //    case 0x78: Load(ref A, B); break;
            //    case 0x79: Load(ref A, C); break;
            //    case 0x7A: Load(ref A, D); break;
            //    case 0x7B: Load(ref A, E); break;
            //    case 0x7C: Load(ref A, H); break;
            //    case 0x7D: Load(ref A, L); break;
            //    case 0x7E: Load(ref A, AddrHL); break;
            //    case 0x2A: Load(ref A, AddrHLI); break;
            //    case 0x3A: Load(ref A, AddrHLD); break;
            //    case 0x3E: Load(ref A, Imm8()); break;
            //    case 0xF2: Load(ref A, ZeroPageC); break;
            //    case 0xF0: Load(ref A, ZeroPage); break;
            //    case 0x0A: Load(ref A, AddrBC); break;
            //    case 0x1A: Load(ref A, AddrDE); break;
            //    case 0xFA: Load(ref A, ReadByte(Imm16())); break;
            //    case 0x47: Load(ref B, A); break;
            //    case 0x40: Load(ref B, B); break;
            //    case 0x41: Load(ref B, C); break;
            //    case 0x42: Load(ref B, D); break;
            //    case 0x43: Load(ref B, E); break;
            //    case 0x44: Load(ref B, H); break;
            //    case 0x45: Load(ref B, L); break;
            //    case 0x46: Load(ref B, AddrHL); break;
            //    case 0x06: Load(ref B, Imm8()); break;
            //    case 0x4F: Load(ref C, A); break;
            //    case 0x48: Load(ref C, B); break;
            //    case 0x49: Load(ref C, C); break;
            //    case 0x4A: Load(ref C, D); break;
            //    case 0x4B: Load(ref C, E); break;
            //    case 0x4C: Load(ref C, H); break;
            //    case 0x4D: Load(ref C, L); break;
            //    case 0x4E: Load(ref C, AddrHL); break;
            //    case 0x0E: Load(ref C, Imm8()); break;
            //    case 0x57: Load(ref D, A); break;
            //    case 0x50: Load(ref D, B); break;
            //    case 0x51: Load(ref D, C); break;
            //    case 0x52: Load(ref D, D); break;
            //    case 0x53: Load(ref D, E); break;
            //    case 0x54: Load(ref D, H); break;
            //    case 0x55: Load(ref D, L); break;
            //    case 0x56: Load(ref D, AddrHL); break;
            //    case 0x16: Load(ref D, Imm8()); break;
            //    case 0x5F: Load(ref E, A); break;
            //    case 0x58: Load(ref E, B); break;
            //    case 0x59: Load(ref E, C); break;
            //    case 0x5A: Load(ref E, D); break;
            //    case 0x5B: Load(ref E, E); break;
            //    case 0x5C: Load(ref E, H); break;
            //    case 0x5D: Load(ref E, L); break;
            //    case 0x5E: Load(ref E, AddrHL); break;
            //    case 0x1E: Load(ref E, Imm8()); break;
            //    case 0x67: Load(ref H, A); break;
            //    case 0x60: Load(ref H, B); break;
            //    case 0x61: Load(ref H, C); break;
            //    case 0x62: Load(ref H, D); break;
            //    case 0x63: Load(ref H, E); break;
            //    case 0x64: Load(ref H, H); break;
            //    case 0x65: Load(ref H, L); break;
            //    case 0x66: Load(ref H, AddrHL); break;
            //    case 0x6F: Load(ref L, A); break;
            //    case 0x68: Load(ref L, B); break;
            //    case 0x69: Load(ref L, C); break;
            //    case 0x6A: Load(ref L, D); break;
            //    case 0x6B: Load(ref L, E); break;
            //    case 0x6C: Load(ref L, H); break;
            //    case 0x6D: Load(ref L, L); break;
            //    case 0x6E: Load(ref L, AddrHL); break;
            //    case 0x2E: Load(ref L, Imm8()); break;
            //    case 0x77: Load(HL, A); break;
            //    case 0x70: Load(HL, B); break;
            //    case 0x71: Load(HL, C); break;
            //    case 0x72: Load(HL, D); break;
            //    case 0x73: Load(HL, E); break;
            //    case 0x74: Load(HL, H); break;
            //    case 0x75: Load(HL, L); break;
            //    case 0x36: Load(HL, Imm8()); break;
            //    case 0x22: Load(HL, A, 1); break;
            //    case 0x32: Load(HL, A, -1); break;
            //    case 0xE2: Load(C + 0xFF00, A); break;
            //    case 0xE0: Load(Imm8() + 0xFF00, A); break;
            //    case 0xEA: Load(Imm16(), A); break;
            //    case 0x02: Load(BC, A); break;
            //    case 0x12: Load(DE, A); break;
            //    #endregion
            //    #region 8-bit arithmetic
            //    case 0x17: Rla(); break;
            //    case 0x3C: Inc(ref A); break;
            //    case 0x04: Inc(ref B); break;
            //    case 0x0C: Inc(ref C); break;
            //    case 0x14: Inc(ref D); break;
            //    case 0x1C: Inc(ref E); break;
            //    case 0x24: Inc(ref H); break;
            //    case 0x2C: Inc(ref L); break;
            //    case 0x34: Inc(HL); break;
            //    case 0x3D: Dec(ref A); break;
            //    case 0x05: Dec(ref B); break;
            //    case 0x0D: Dec(ref C); break;
            //    case 0x15: Dec(ref D); break;
            //    case 0x1D: Dec(ref E); break;
            //    case 0x25: Dec(ref H); break;
            //    case 0x2D: Dec(ref L); break;
            //    case 0x35: Dec(HL); break;
            //    case 0xA7: And(A); break;
            //    case 0xA0: And(B); break;
            //    case 0xA1: And(C); break;
            //    case 0xA2: And(D); break;
            //    case 0xA3: And(E); break;
            //    case 0xA4: And(H); break;
            //    case 0xA5: And(L); break;
            //    case 0xA6: And(AddrHL); break;
            //    case 0xE6: And(Imm8()); break;
            //    case 0xAF: Xor(A); break;
            //    case 0xA8: Xor(B); break;
            //    case 0xA9: Xor(C); break;
            //    case 0xAA: Xor(D); break;
            //    case 0xAB: Xor(E); break;
            //    case 0xAC: Xor(H); break;
            //    case 0xAD: Xor(L); break;
            //    case 0xAE: Xor(AddrHL); break;
            //    case 0xB7: Or(A); break;
            //    case 0xB0: Or(B); break;
            //    case 0xB1: Or(C); break;
            //    case 0xB2: Or(D); break;
            //    case 0xB3: Or(E); break;
            //    case 0xB4: Or(H); break;
            //    case 0xB5: Or(L); break;
            //    case 0xB6: Or(AddrHL); break;
            //    case 0x87: Add(A); break;
            //    case 0x80: Add(B); break;
            //    case 0x81: Add(C); break;
            //    case 0x82: Add(D); break;
            //    case 0x83: Add(E); break;
            //    case 0x84: Add(H); break;
            //    case 0x85: Add(L); break;
            //    case 0x86: Add(AddrHL); break;
            //    case 0x8E: Adc(AddrHL); break;
            //    case 0x97: Sub(A); break;
            //    case 0x90: Sub(B); break;
            //    case 0x91: Sub(C); break;
            //    case 0x92: Sub(D); break;
            //    case 0x93: Sub(E); break;
            //    case 0x94: Sub(H); break;
            //    case 0x95: Sub(L); break;
            //    case 0x96: Sub(AddrHL); break;
            //    case 0xBE: Cp(AddrHL); break;
            //    case 0xFE: Cp(Imm8()); break;
            //    #endregion
            //    #region Control                
            //    case 0x18: Jr(); break;
            //    case 0x20: Jr(Cond.NZ); break;
            //    case 0x28: Jr(Cond.Z); break;
            //    case 0xC3: Jp(); break;
            //    case 0xCA: Jp(Cond.Z); break;
            //    case 0xE9: JpHL(); break;
            //    case 0xCD: Call(); break;
            //    case 0xC0: Ret(Cond.NZ); break;
            //    case 0xC8: Ret(Cond.Z); break;
            //    case 0xD0: Ret(Cond.NC); break;
            //    case 0xD8: Ret(Cond.C); break;
            //    case 0xC9: Ret(); break;
            //    case 0xD9: Reti(); return;
            //    case 0xC7: Rst(0x00); break;
            //    case 0xCF: Rst(0x08); break;
            //    case 0xD7: Rst(0x10); break;
            //    case 0xDF: Rst(0x18); break;
            //    case 0xE7: Rst(0x20); break;
            //    case 0xEF: Rst(0x28); break;
            //    case 0xF7: Rst(0x30); break;
            //    case 0xFF: Rst(0x38); break;
            //    #endregion
            //    #region Miscellaneous
            //    case 0x00: Nop(); break;
            //    case 0x27: Daa(); break;
            //    case 0x2F: Cpl(); break;
            //    case 0xCB: DecodeCB(DecodeInstruction()); break;
            //    case 0xF3: Di(); break;
            //    case 0xFB: Ei(); break;
            //    #endregion
            //    #region 16-bit loads
            //    case 0x01: Load16(ref BC); break;
            //    case 0x11: Load16(ref DE); break;
            //    case 0x21: Load16(ref HL); break;
            //    case 0x31: Load16(ref SP); break;
            //    case 0xC5: Push16(BC); break;
            //    case 0xD5: Push16(DE); break;
            //    case 0xE5: Push16(HL); break;
            //    case 0xF5: Push16(AF); break;
            //    case 0xC1: Pop16(ref BC); break;
            //    case 0xD1: Pop16(ref DE); break;
            //    case 0xE1: Pop16(ref HL); break;
            //    case 0xF1: Pop16(ref AF); break;
            //    #endregion
            //    #region 16-bit arithmetic
            //    case 0x03: Inc16(ref BC); break;
            //    case 0x13: Inc16(ref DE); break;
            //    case 0x23: Inc16(ref HL); break;
            //    case 0x33: Inc16(ref SP); break;
            //    case 0x0B: Dec16(ref BC); break;
            //    case 0x1B: Dec16(ref DE); break;
            //    case 0x2B: Dec16(ref HL); break;
            //    case 0x3B: Dec16(ref SP); break;
            //    case 0x09: Add16(BC); break;
            //    case 0x19: Add16(DE); break;
            //    case 0x29: Add16(HL); break;
            //    case 0x39: Add16(SP); break;
            //    #endregion

            //    default: Unimplemented(op); break;
            //}
            #endregion
        }

        private void DecodeCB()
        {
            var op = DecodeInstruction();

            int z = (op & 0b00_000_111) >> 0;
            int y = (op & 0b00_111_000) >> 3;
            int x = (op & 0b11_000_000) >> 6;
            int p = (op & 0b00_110_000) >> 4;
            int q = (op & 0b00_001_000) >> 3;

            switch (x)
            {
            }

            #region Deprecated DecodeCB Logic
            //switch (op)
            //{
            //    #region Rotate Left
            //    case 0x11: Rl(ref C); break;
            //    #endregion
            //    #region Swap
            //    case 0x37: Swap(ref A); break;
            //    case 0x30: Swap(ref B); break;
            //    case 0x31: Swap(ref C); break;
            //    case 0x32: Swap(ref D); break;
            //    case 0x33: Swap(ref E); break;
            //    case 0x34: Swap(ref H); break;
            //    case 0x35: Swap(ref L); break;
            //    case 0x36: SwapHL(); break;
            //    #endregion
            //    #region Bit
            //    case 0x7C: Bit(7, H); break;
            //    case 0x41: Bit(0, C); break;
            //    #endregion
            //    #region Reset
            //    case 0x87: Res(0, ref A); break;
            //    case 0xBE: Res(7, HL); break;
            //    #endregion

            //    default: UnimplementedCB(op); break;
            //}
            #endregion
        }

        private Exception Unimplemented(byte op)
        {
            return new Exception($"Opcode not defined: 0x{op:X2} at PC: 0x{PC - 1:X4}.");
        }

        private void UnimplementedCB(byte op)
        {
            throw new Exception($"Opcode not defined: 0xCB 0x{op:X2} at PC: 0x{PC - 2:X4}.");
        }
    }
}