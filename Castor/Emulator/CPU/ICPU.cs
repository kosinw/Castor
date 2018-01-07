using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public interface ICPU
    {
        // --- 8-bit operations
        // 8-bit loads
        void Load(ref byte out8, byte in8);
        void Load(int indr, byte in8, int hlAction = 0);
        // 8-bit arithmetic
        void Add(byte in8);
        void Adc(byte in8);
        void Sub(byte in8);
        void Sbc(byte in8);
        void Cp(byte in8);
        void And(byte in8);
        void Or(byte in8);
        void Xor(byte in8);
        void Inc(ref byte io8);
        void Inc(int indr);
        void Dec(ref byte io8);
        void Rlca();
        void Rla();
        void Rrca();
        void Rra();
        void Rlc(ref byte io8);
        void Rl(ref byte io8);
        void Rrc(ref byte io8);
        void Rr(ref byte io8);
        void Sla(ref byte io8);
        void Sra(ref byte io8);
        void Srl(ref byte io8);
        void Swap(ref byte io8);
        void SwapHL();
        void Bit(int num, byte in8);
        void Set(int num, ref byte io8);
        void Res(int num, ref byte io8);
        // --- Control
        void Jp();
        void JpHL();
        void Jr();
        void Call();
        void Ret();
        void Reti();
        void Jp(Cond cond);
        void Jr(Cond cond);
        void Call(Cond cond);
        void Ret(Cond cond);
        void Rst(byte vec);
        // --- Miscellaneous
        void Halt();
        void Stop();
        void Di();
        void Ei();
        void Ccf();
        void Scf();
        void Nop();
        void Daa();
        void Cpl();
        // --- 16-bit operations
        // 16-bit loads
        void Load16(ref ushort io16);
        void Load16IndrSP();
        void Load16SPHL();
        void Load16HLSPe();
        void Push16(ushort io16);
        void Pop16(ref ushort io16);
        // 16-bit arithmetic
        void Add16(ushort io16);
        void Add16SPe();
        void Inc16(ref ushort io16);
        void Dec16(ref ushort io16);
    }
}
