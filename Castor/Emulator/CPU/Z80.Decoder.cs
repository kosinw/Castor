using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {        
        public void Decode(Device d, byte op)
        {
            switch (op)
            {
                #region 8-bit loads
                case 0x7F: Load(ref A, A); break;
                case 0x78: Load(ref A, B); break;
                case 0x79: Load(ref A, C); break;
                case 0x7A: Load(ref A, D); break;
                case 0x7B: Load(ref A, E); break;
                case 0x7C: Load(ref A, H); break;
                case 0x7D: Load(ref A, L); break;
                case 0x7E: Load(ref A, AddrHL); break;
                case 0x2A: Load(ref A, AddrHLI); break;
                case 0x3A: Load(ref A, AddrHLD); break;
                case 0x3E: Load(ref A, Imm8()); break;
                case 0xF2: Load(ref A, ZeroPageC); break;
                case 0xF0: Load(ref A, ZeroPage); break;
                case 0x0A: Load(ref A, AddrBC); break;
                case 0x1A: Load(ref A, AddrDE); break;
                case 0x47: Load(ref B, A); break;
                case 0x40: Load(ref B, B); break;
                case 0x41: Load(ref B, C); break;
                case 0x42: Load(ref B, D); break;
                case 0x43: Load(ref B, E); break;
                case 0x44: Load(ref B, H); break;
                case 0x45: Load(ref B, L); break;
                case 0x46: Load(ref B, AddrHL); break;
                case 0x06: Load(ref B, Imm8()); break;
                case 0x4F: Load(ref C, A); break;
                case 0x48: Load(ref C, B); break;
                case 0x49: Load(ref C, C); break;
                case 0x4A: Load(ref C, D); break;
                case 0x4B: Load(ref C, E); break;
                case 0x4C: Load(ref C, H); break;
                case 0x4D: Load(ref C, L); break;
                case 0x4E: Load(ref C, AddrHL); break;
                case 0x0E: Load(ref C, Imm8()); break;
                case 0x57: Load(ref D, A); break;
                case 0x50: Load(ref D, B); break;
                case 0x51: Load(ref D, C); break;
                case 0x52: Load(ref D, D); break;
                case 0x53: Load(ref D, E); break;
                case 0x54: Load(ref D, H); break;
                case 0x55: Load(ref D, L); break;
                case 0x56: Load(ref D, AddrHL); break;
                case 0x16: Load(ref D, Imm8()); break;
                case 0x5F: Load(ref E, A); break;
                case 0x58: Load(ref E, B); break;
                case 0x59: Load(ref E, C); break;
                case 0x5A: Load(ref E, D); break;
                case 0x5B: Load(ref E, E); break;
                case 0x5C: Load(ref E, H); break;
                case 0x5D: Load(ref E, L); break;
                case 0x5E: Load(ref E, AddrHL); break;
                case 0x1E: Load(ref E, Imm8()); break;
                case 0x67: Load(ref H, A); break;
                case 0x60: Load(ref H, B); break;
                case 0x61: Load(ref H, C); break;
                case 0x62: Load(ref H, D); break;
                case 0x63: Load(ref H, E); break;
                case 0x64: Load(ref H, H); break;
                case 0x65: Load(ref H, L); break;
                case 0x66: Load(ref H, AddrHL); break;
                case 0x6F: Load(ref L, A); break;
                case 0x68: Load(ref L, B); break;
                case 0x69: Load(ref L, C); break;
                case 0x6A: Load(ref L, D); break;
                case 0x6B: Load(ref L, E); break;
                case 0x6C: Load(ref L, H); break;
                case 0x6D: Load(ref L, L); break;
                case 0x6E: Load(ref L, AddrHL); break;
                case 0x2E: Load(ref L, Imm8()); break;
                case 0x77: Load(HL, A); break;
                case 0x70: Load(HL, B); break;
                case 0x71: Load(HL, C); break;
                case 0x72: Load(HL, D); break;
                case 0x73: Load(HL, E); break;
                case 0x74: Load(HL, H); break;
                case 0x75: Load(HL, L); break;
                case 0x36: Load(HL, Imm8()); break;
                case 0x22: Load(HL, A, 1); break;
                case 0x32: Load(HL, A, -1); break;
                case 0xE2: Load(C + 0xFF00, A); break;
                case 0xE0: Load(Imm8() + 0xFF00, A); break;
                case 0xEA: Load(Imm16(), A); break;
                #endregion
                #region 8-bit arithmetic
                case 0x17: Rla(); break;
                case 0x3C: Inc(ref A); break;
                case 0x04: Inc(ref B); break;
                case 0x0C: Inc(ref C); break;
                case 0x14: Inc(ref D); break;
                case 0x1C: Inc(ref E); break;
                case 0x24: Inc(ref H); break;
                case 0x2C: Inc(ref L); break;
                case 0x34: Inc(HL); break;
                case 0x3D: Dec(ref A); break;
                case 0x05: Dec(ref B); break;
                case 0x0D: Dec(ref C); break;
                case 0x15: Dec(ref D); break;
                case 0x1D: Dec(ref E); break;
                case 0x25: Dec(ref H); break;
                case 0x2D: Dec(ref L); break;
                case 0x35: Dec(HL); break;
                case 0xA7: And(A); break;
                case 0xA0: And(B); break;
                case 0xA1: And(C); break;
                case 0xA2: And(D); break;
                case 0xA3: And(E); break;
                case 0xA4: And(H); break;
                case 0xA5: And(L); break;
                case 0xA6: And(AddrHL); break;
                case 0xAF: Xor(A); break;
                case 0xA8: Xor(B); break;
                case 0xA9: Xor(C); break;
                case 0xAA: Xor(D); break;
                case 0xAB: Xor(E); break;
                case 0xAC: Xor(H); break;
                case 0xAD: Xor(L); break;
                case 0xAE: Xor(AddrHL); break;
                case 0xB7: Or(A); break;
                case 0xB0: Or(B); break;
                case 0xB1: Or(C); break;
                case 0xB2: Or(D); break;
                case 0xB3: Or(E); break;
                case 0xB4: Or(H); break;
                case 0xB5: Or(L); break;
                case 0xB6: Or(AddrHL); break;
                case 0x87: Add(A); break;
                case 0x80: Add(B); break;
                case 0x81: Add(C); break;
                case 0x82: Add(D); break;
                case 0x83: Add(E); break;
                case 0x84: Add(H); break;
                case 0x85: Add(L); break;
                case 0x86: Add(AddrHL); break;
                case 0x97: Sub(A); break;
                case 0x90: Sub(B); break;
                case 0x91: Sub(C); break;
                case 0x92: Sub(D); break;
                case 0x93: Sub(E); break;
                case 0x94: Sub(H); break;
                case 0x95: Sub(L); break;
                case 0x96: Sub(AddrHL); break;
                case 0xBE: Cp(AddrHL); break;
                case 0xFE: Cp(Imm8()); break;
                #endregion
                #region Control                
                case 0x18: Jr(); break;
                case 0x20: Jr(Cond.NZ); break;
                case 0x28: Jr(Cond.Z); break;
                case 0xC3: Jp(); break;
                case 0xCD: Call(); break;
                case 0xC9: Ret(); break;
                #endregion
                #region Miscellaneous
                case 0x00: Nop(); break;
                case 0x2F: Cpl(); break;
                case 0xCB: DecodeCB(d, DecodeInstruction()); break;
                case 0xF3: Di(); break;
                case 0xFB: Ei(); break;
                #endregion
                #region 16-bit loads
                case 0x01: Load16(ref BC); break;
                case 0x11: Load16(ref DE); break;
                case 0x21: Load16(ref HL); break;
                case 0x31: Load16(ref SP); break;
                case 0xC5: Push16(BC); break;
                case 0xD5: Push16(DE); break;
                case 0xE5: Push16(HL); break;
                case 0xF5: Push16(AF); break;
                case 0xC1: Pop16(ref BC); break;
                case 0xD1: Pop16(ref DE); break;
                case 0xE1: Pop16(ref HL); break;
                case 0xF1: Pop16(ref AF); break;
                #endregion
                #region 16-bit arithmetic
                case 0x03: Inc16(ref BC); break;
                case 0x13: Inc16(ref DE); break;
                case 0x23: Inc16(ref HL); break;
                case 0x33: Inc16(ref SP); break;
                case 0x0B: Dec16(ref BC); break;
                case 0x1B: Dec16(ref DE); break;
                case 0x2B: Dec16(ref HL); break;
                case 0x3B: Dec16(ref SP); break;
                #endregion

                default: Unimplemented(d, op); break;
            }
        }

        private void DecodeCB(Device d, byte op)
        {
            switch (op)
            {
                #region Rotate Left
                case 0x11: Rl(ref C); break;
                #endregion
                #region Bit
                case 0x7C: Bit(7, H); break;
                #endregion

                default: UnimplementedCB(d, op); break;
            }
        }

        private void Unimplemented(Device d, byte op)
        {
            throw new Exception($"Opcode not defined: 0x{op:X2} at PC: 0x{PC - 1:X2}.");
        }

        private void UnimplementedCB(Device d, byte op)
        {
            throw new Exception($"Opcode not defined: 0xCB 0x{op:X2} at PC: 0x{PC - 2:X2}.");
        }
    }
}