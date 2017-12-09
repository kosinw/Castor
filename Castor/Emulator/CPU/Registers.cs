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
        [FieldOffset(0)] public byte A;
        [FieldOffset(1)] public byte F;
        [FieldOffset(2)] public byte B;
        [FieldOffset(3)] public byte C;
        [FieldOffset(4)] public byte D;
        [FieldOffset(5)] public byte E;
        [FieldOffset(6)] public byte H;
        [FieldOffset(7)] public byte L;

        [FieldOffset(0)] public ushort AF;
        [FieldOffset(2)] public ushort BC;
        [FieldOffset(4)] public ushort DE;
        [FieldOffset(6)] public ushort HL;

        [FieldOffset(8)] public ushort SP;
        [FieldOffset(10)] public ushort PC;        
    }
}
