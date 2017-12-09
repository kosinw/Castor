using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public abstract class OpcodeBuilder
    {
        // --- 8-bit operations
        // 8-bit loads
        public abstract void Load(ref byte out8, byte in8);
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
        public abstract void Load16SP();
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
                // --- 8-bit operations
                // 8-bit loads
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
            }
        }
    }
}
