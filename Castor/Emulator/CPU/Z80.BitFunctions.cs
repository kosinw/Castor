using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Math = Castor.Emulator.Utility.Math;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        private void PopulateBitwiseInstructions()
        {
            // SWAP
            _cb[0x30] = SWAP(() => B = Math.Swap(B));
            _cb[0x31] = SWAP(() => C = Math.Swap(C));
            _cb[0x32] = SWAP(() => D = Math.Swap(D));
            _cb[0x33] = SWAP(() => E = Math.Swap(E));
            _cb[0x34] = SWAP(() => H = Math.Swap(H));
            _cb[0x35] = SWAP(() => L = Math.Swap(L));
            _cb[0x36] = SWAP(() => AddrHL = Math.Swap(AddrHL));
            _cb[0x37] = SWAP(() => A = Math.Swap(A));

            // RES 
            for (int i = 0; i < 8; ++i)
            {
                _cb[0x80 + (i << 3)] = RES(() => B = Math.Reset(B, i));
                _cb[0x81 + (i << 3)] = RES(() => C = Math.Reset(C, i));
                _cb[0x82 + (i << 3)] = RES(() => D = Math.Reset(D, i));
                _cb[0x83 + (i << 3)] = RES(() => E = Math.Reset(E, i));
                _cb[0x84 + (i << 3)] = RES(() => H = Math.Reset(H, i));
                _cb[0x85 + (i << 3)] = RES(() => L = Math.Reset(L, i));
                _cb[0x86 + (i << 3)] = RES(() => AddrHL = Math.Reset(AddrHL, i));
                _cb[0x87 + (i << 3)] = RES(() => A = Math.Reset(A, i));
            }
        }

        /// <summary>
        /// A shorthand notation to reigster RES instructions.
        /// <param name="fn"></param>
        /// </summary>
        Instruction RES(Action fn)
        {
            return delegate
            {
                fn.Invoke();
            };
        }


        /// <summary>
        /// A shorthand notation to register SWAP instructions.
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        Instruction SWAP(Func<int> fn)
        {
            return delegate
            {
                int result = fn.Invoke();

                SetFlag(result == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
                SetFlag(false, StatusFlags.C);
            };
        }
    }
}
