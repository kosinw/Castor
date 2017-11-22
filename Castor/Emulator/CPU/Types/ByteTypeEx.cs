using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU.Types
{
    public static class ByteTypeEx
    {
        /// <summary>
        /// An enum that represents any of the 8 bit registers on the Z80.
        /// </summary>
        public enum ByteType : int
        {
            A,
            B,
            C,
            D,
            E,
            H,
            L,
            _HL,
            F,
            Imm8,
            Addr8,
            Addr16,            
            _C,
            _HLI,
            _HLD,
            _BC,
            _DE,
        }

        /// <summary>
        /// Return the length of the parameter.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int Length(this ByteType t)
        {
            if (t == ByteType.Addr16)
                return 2;
            else if (t == ByteType.Addr8 || t == ByteType.Imm8)
                return 1;

            return 0;
        }

        /// <summary>
        /// Return the amount the CPU has to delay by accessing this type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int Delay(this ByteType t)
        {
            if (t == ByteType.Addr16)
                return 3;
            else if (t == ByteType.Addr8)
                return 2;
            else if (t == ByteType._C || t == ByteType._HL)
                return 1;
            else if (t == ByteType._HLD || t == ByteType._HLI)
                return 1;
            else if (t == ByteType._BC || t == ByteType._DE)
                return 1;

            return 0;
        }
    }
}
