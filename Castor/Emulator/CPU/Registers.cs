using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Registers
    {
        [FieldOffset(1)] public byte A;
        [FieldOffset(0)] public byte F;

        [FieldOffset(3)] public byte B;
        [FieldOffset(2)] public byte C;

        [FieldOffset(5)] public byte D;
        [FieldOffset(4)] public byte E;

        [FieldOffset(7)] public byte H;
        [FieldOffset(6)] public byte L;

        [FieldOffset(0)] public ushort AF;
        [FieldOffset(2)] public ushort BC;
        [FieldOffset(4)] public ushort DE;
        [FieldOffset(6)] public ushort HL;

        [FieldOffset(12)] public ushort SP;
        [FieldOffset(16)] public ushort PC;

        public ushort Bump(ushort times = 1)
        {
            var ret = PC;
            PC += times;
            return ret;
        }
    }
}
