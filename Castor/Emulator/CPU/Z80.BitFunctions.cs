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
            _cb[0x30] = RSWI(() => B = Math.Swap(B));
            _cb[0x31] = RSWI(() => C = Math.Swap(C));
            _cb[0x32] = RSWI(() => D = Math.Swap(D));
            _cb[0x33] = RSWI(() => E = Math.Swap(E));
            _cb[0x34] = RSWI(() => H = Math.Swap(H));
            _cb[0x35] = RSWI(() => L = Math.Swap(L));
            _cb[0x36] = RSWI(() => AddrHL = Math.Swap(AddrHL));
            _cb[0x37] = RSWI(() => A = Math.Swap(A));
        }


        /// <summary>
        /// A shorthand notation to register SWAP instructions.
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        Instruction RSWI(Func<int> fn)
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
