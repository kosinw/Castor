using Castor.Emulator.CPU;
using Castor.Emulator.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public static class Memory
    {
        public static MemoryReference<byte> GetReference(this MemoryMapper mmu, int addr)
        {
            return new MemoryReference<byte>
            {
                Read = () => mmu[addr],
                Write = v => mmu[addr] = v
            };
        }

        #region 8-bit Registers
        public static MemoryReference<byte> RegisterA(this Z80 cpu)
        {
            return new MemoryReference<byte>
            {
                Read = () => cpu.A,
                Write = v => cpu.A = v
            };
        }

        public static MemoryReference<byte> RegisterB(this Z80 cpu)
        {
            return new MemoryReference<byte>
            {
                Read = () => cpu.B,
                Write = v => cpu.B = v
            };
        }

        public static MemoryReference<byte> RegisterC(this Z80 cpu)
        {
            return new MemoryReference<byte>
            {
                Read = () => cpu.C,
                Write = v => cpu.C = v
            };
        }

        public static MemoryReference<byte> RegisterD(this Z80 cpu)
        {
            return new MemoryReference<byte>
            {
                Read = () => cpu.D,
                Write = v => cpu.D = v
            };
        }

        public static MemoryReference<byte> RegisterE(this Z80 cpu)
        {
            return new MemoryReference<byte>
            {
                Read = () => cpu.E,
                Write = v => cpu.E = v
            };
        }

        public static MemoryReference<byte> RegisterF(this Z80 cpu)
        {
            return new MemoryReference<byte>
            {
                Read = () => cpu.F,
                Write = v => cpu.F = v
            };
        }

        public static MemoryReference<byte> RegisterH(this Z80 cpu)
        {
            return new MemoryReference<byte>
            {
                Read = () => cpu.H,
                Write = v => cpu.H = v
            };
        }

        public static MemoryReference<byte> RegisterL(this Z80 cpu)
        {
            return new MemoryReference<byte>
            {
                Read = () => cpu.L,
                Write = v => cpu.L = v
            };
        }
        #endregion

        #region 16-bit Registers
        public static MemoryReference<ushort> RegisterSP(this Z80 cpu)
        {
            return new MemoryReference<ushort>
            {
                Read = () => cpu.SP,
                Write = v => cpu.SP = v
            };
        }

        public static MemoryReference<ushort> RegisterPC(this Z80 cpu)
        {
            return new MemoryReference<ushort>
            {
                Read = () => cpu.PC,
                Write = v => cpu.PC = v
            };
        }

        public static MemoryReference<ushort> RegisterAF(this Z80 cpu)
        {
            return new MemoryReference<ushort>
            {
                Read = () => cpu.AF,
                Write = v => cpu.AF = v
            };
        }

        public static MemoryReference<ushort> RegisterBC(this Z80 cpu)
        {
            return new MemoryReference<ushort>
            {
                Read = () => cpu.BC,
                Write = v => cpu.BC = v
            };
        }

        public static MemoryReference<ushort> RegisterDE(this Z80 cpu)
        {
            return new MemoryReference<ushort>
            {
                Read = () => cpu.DE,
                Write = v => cpu.DE = v
            };
        }

        public static MemoryReference<ushort> RegisterHL(this Z80 cpu)
        {
            return new MemoryReference<ushort>
            {
                Read = () => cpu.HL,
                Write = v => cpu.HL = v
            };
        }
        #endregion
    }
}
