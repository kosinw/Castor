using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public struct Reg8 : IRead<byte>, IWrite<byte>
    {
        public Func<byte> ReadFn;
        public Action<byte> WriteFn;

        public byte Read()
        {
            return ReadFn.Invoke();
        }

        public void Write(byte value)
        {
            WriteFn.Invoke(value);
        }

        public static Reg8 operator ++(Reg8 r8)
        {
            r8.Write((byte)(r8.Read() + 1));
            return r8;
        }

        public static Reg8 operator --(Reg8 r8)
        {
            r8.Write((byte)(r8.Read() - 1));
            return r8;
        }
    }

    public struct Reg16 : IRead<ushort>, IWrite<ushort>
    {
        public Func<ushort> ReadFn;
        public Action<ushort> WriteFn;

        public ushort Read()
        {
            return ReadFn.Invoke();
        }

        public void Write(ushort value)
        {
            WriteFn.Invoke(value);
        }

        public static Reg16 operator ++(Reg16 r16)
        {
            r16.Write((byte)(r16.Read() + 1));
            return r16;
        }

        public static Reg16 operator --(Reg16 r16)
        {
            r16.Write((byte)(r16.Read() - 1));
            return r16;
        }
    }
}
