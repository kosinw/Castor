using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IA = Castor.Emulator.CPU.IndirectAddress;

namespace Castor.Emulator.CPU
{
    public abstract class OpcodeBuilder
    {
        // --- 8-bit operations
        // 8-bit loads
        public abstract void Load(ref byte out8, byte in8);
        public abstract void Load(ushort indr, byte in8, int hlAction = 0);
        // 8-bit arithmetic
        public abstract void Add(byte in8);
        public abstract void Adc(byte in8);
        public abstract void Sub(byte in8);
        public abstract void Sbc(byte in8);
        public abstract void Cp(byte in8);
        public abstract void And(byte in8);
        public abstract void Or(byte in8);
        public abstract void Xor(byte in8);
        public abstract void Inc(ref byte io8);
        public abstract void Dec(ref byte io8);
        public abstract void Rlca();
        public abstract void Rla();
        public abstract void Rrca();
        public abstract void Rra();
        public abstract void Rlc(ref byte io8);
        public abstract void Rl(ref byte io8);
        public abstract void Rrc(ref byte io8);
        public abstract void Rr(ref byte io8);
        public abstract void Sla(ref byte io8);
        public abstract void Sra(ref byte io8);
        public abstract void Srl(ref byte io8);
        public abstract void Swap(ref byte io8);
        public abstract void Bit(uint num, byte in8);
        public abstract void Set(uint num, ref byte io8);
        public abstract void Res(uint num, ref byte io8);
        // --- Control
        public abstract void Jp();
        public abstract void JpHL();
        public abstract void Jr();
        public abstract void Call();
        public abstract void Ret();
        public abstract void Reti();
        public abstract void Jp(Cond cond);
        public abstract void Jr(Cond cond);
        public abstract void Call(Cond cond);
        public abstract void Ret(Cond cond);
        public abstract void Rst(byte vec);
        // --- Miscellaneous
        public abstract void Halt();
        public abstract void Stop();
        public abstract void Di();
        public abstract void Ei();
        public abstract void Ccf();
        public abstract void Scf();
        public abstract void Nop();
        public abstract void Daa();
        public abstract void Cpl();
        public abstract void CbPrefix();
        // --- 16-bit operations
        // 16-bit loads
        public abstract void Load16(ref ushort io16);
        public abstract void Load16IndrSP();
        public abstract void Load16SPHL();
        public abstract void Load16HLSPe();
        public abstract void Push16(ref ushort io16);
        public abstract void Pop16(ref ushort io16);
        // 16-bit arithmetic
        public abstract void Add16(ref ushort io16);
        public abstract void Add16SPe();
        public abstract void Inc16(ref ushort io16);
        public abstract void Dec16(ref ushort io16);

        public void Decode(Device d, byte op)
        {
            switch (op)
            {
                #region 8-bit loads
                case 0x7F: Load(ref d.CPU.A, d.CPU.A); break;
                case 0x78: Load(ref d.CPU.A, d.CPU.B); break;
                case 0x79: Load(ref d.CPU.A, d.CPU.C); break;
                case 0x7A: Load(ref d.CPU.A, d.CPU.D); break;
                case 0x7B: Load(ref d.CPU.A, d.CPU.E); break;
                case 0x7C: Load(ref d.CPU.A, d.CPU.H); break;
                case 0x7D: Load(ref d.CPU.A, d.CPU.L); break;
                case 0x7E: Load(ref d.CPU.A, d.CPU.AddrHL); break;
                case 0x47: Load(ref d.CPU.B, d.CPU.A); break;
                case 0x40: Load(ref d.CPU.B, d.CPU.B); break;
                case 0x41: Load(ref d.CPU.B, d.CPU.C); break;
                case 0x42: Load(ref d.CPU.B, d.CPU.D); break;
                case 0x43: Load(ref d.CPU.B, d.CPU.E); break;
                case 0x44: Load(ref d.CPU.B, d.CPU.H); break;
                case 0x45: Load(ref d.CPU.B, d.CPU.L); break;
                case 0x46: Load(ref d.CPU.B, d.CPU.AddrHL); break;
                case 0x4F: Load(ref d.CPU.C, d.CPU.A); break;
                case 0x48: Load(ref d.CPU.C, d.CPU.B); break;
                case 0x49: Load(ref d.CPU.C, d.CPU.C); break;
                case 0x4A: Load(ref d.CPU.C, d.CPU.D); break;
                case 0x4B: Load(ref d.CPU.C, d.CPU.E); break;
                case 0x4C: Load(ref d.CPU.C, d.CPU.H); break;
                case 0x4D: Load(ref d.CPU.C, d.CPU.L); break;
                case 0x4E: Load(ref d.CPU.C, d.CPU.AddrHL); break;
                case 0x57: Load(ref d.CPU.D, d.CPU.A); break;
                case 0x50: Load(ref d.CPU.D, d.CPU.B); break;
                case 0x51: Load(ref d.CPU.D, d.CPU.C); break;
                case 0x52: Load(ref d.CPU.D, d.CPU.D); break;
                case 0x53: Load(ref d.CPU.D, d.CPU.E); break;
                case 0x54: Load(ref d.CPU.D, d.CPU.H); break;
                case 0x55: Load(ref d.CPU.D, d.CPU.L); break;
                case 0x56: Load(ref d.CPU.D, d.CPU.AddrHL); break;
                case 0x5F: Load(ref d.CPU.E, d.CPU.A); break;
                case 0x58: Load(ref d.CPU.E, d.CPU.B); break;
                case 0x59: Load(ref d.CPU.E, d.CPU.C); break;
                case 0x5A: Load(ref d.CPU.E, d.CPU.D); break;
                case 0x5B: Load(ref d.CPU.E, d.CPU.E); break;
                case 0x5C: Load(ref d.CPU.E, d.CPU.H); break;
                case 0x5D: Load(ref d.CPU.E, d.CPU.L); break;
                case 0x5E: Load(ref d.CPU.E, d.CPU.AddrHL); break;
                case 0x67: Load(ref d.CPU.H, d.CPU.A); break;
                case 0x60: Load(ref d.CPU.H, d.CPU.B); break;
                case 0x61: Load(ref d.CPU.H, d.CPU.C); break;
                case 0x62: Load(ref d.CPU.H, d.CPU.D); break;
                case 0x63: Load(ref d.CPU.H, d.CPU.E); break;
                case 0x64: Load(ref d.CPU.H, d.CPU.H); break;
                case 0x65: Load(ref d.CPU.H, d.CPU.L); break;
                case 0x66: Load(ref d.CPU.H, d.CPU.AddrHL); break;
                case 0x6F: Load(ref d.CPU.L, d.CPU.A); break;
                case 0x68: Load(ref d.CPU.L, d.CPU.B); break;
                case 0x69: Load(ref d.CPU.L, d.CPU.C); break;
                case 0x6A: Load(ref d.CPU.L, d.CPU.D); break;
                case 0x6B: Load(ref d.CPU.L, d.CPU.E); break;
                case 0x6C: Load(ref d.CPU.L, d.CPU.H); break;
                case 0x6D: Load(ref d.CPU.L, d.CPU.L); break;
                case 0x6E: Load(ref d.CPU.L, d.CPU.AddrHL); break;
                case 0x77: Load(d.CPU.HL, d.CPU.A); break;
                case 0x70: Load(d.CPU.HL, d.CPU.B); break;
                case 0x71: Load(d.CPU.HL, d.CPU.C); break;
                case 0x72: Load(d.CPU.HL, d.CPU.D); break;
                case 0x73: Load(d.CPU.HL, d.CPU.E); break;
                case 0x74: Load(d.CPU.HL, d.CPU.H); break;
                case 0x75: Load(d.CPU.HL, d.CPU.L); break;
                case 0x22: Load(d.CPU.HL, d.CPU.A, 1); break;
                case 0x32: Load(d.CPU.HL, d.CPU.A, -1); break;
                #endregion
                #region 8-bit arithmetic
                case 0xA7: And(d.CPU.A); break;
                case 0xA0: And(d.CPU.B); break;
                case 0xA1: And(d.CPU.C); break;
                case 0xA2: And(d.CPU.D); break;
                case 0xA3: And(d.CPU.E); break;
                case 0xA4: And(d.CPU.H); break;
                case 0xA5: And(d.CPU.L); break;
                case 0xA6: And(d.CPU.AddrHL); break;
                case 0xAF: Xor(d.CPU.A); break;
                case 0xA8: Xor(d.CPU.B); break;
                case 0xA9: Xor(d.CPU.C); break;
                case 0xAA: Xor(d.CPU.D); break;
                case 0xAB: Xor(d.CPU.E); break;
                case 0xAC: Xor(d.CPU.H); break;
                case 0xAD: Xor(d.CPU.L); break;
                case 0xAE: Xor(d.CPU.AddrHL); break;
                case 0xB7: Or(d.CPU.A); break;
                case 0xB0: Or(d.CPU.B); break;
                case 0xB1: Or(d.CPU.C); break;
                case 0xB2: Or(d.CPU.D); break;
                case 0xB3: Or(d.CPU.E); break;
                case 0xB4: Or(d.CPU.H); break;
                case 0xB5: Or(d.CPU.L); break;
                case 0xB6: Or(d.CPU.AddrHL); break;
                #endregion
                #region 16-bit loads
                case 0x21: Load16(ref d.CPU.HL); break;
                case 0x31: Load16(ref d.CPU.SP); break;
                #endregion

                default: Unimplemented(d); break;
            }
        }

        private void Unimplemented(Device d)
        {
            throw new Exception($"Opcode not defined: {d.MMU[d.CPU.PC - 1]:X2} at PC: {d.CPU.PC - 1:X2}");
        }
    }
}
