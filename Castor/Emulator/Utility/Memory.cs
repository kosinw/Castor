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
        public static byte ReadByte(this MemoryMapper mmu, int addr) => mmu[addr];
        public static void WriteByte(this MemoryMapper mmu, int addr, byte value) => mmu[addr] = value;

        public static ushort ReadWord(this MemoryMapper mmu, int addr) => (ushort)(mmu[addr + 1] << 8 | mmu[addr]);
        public static void WriteWord(this MemoryMapper mmu, int addr, ushort value)
        {
            mmu[addr] = value.LeastSignificantByte();
            mmu[addr + 1] = value.MostSignificantByte();
        }
    }
}
