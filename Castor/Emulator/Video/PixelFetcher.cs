using Castor.Emulator.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public class PixelFetcher
    {
        private MemoryMapper _mmu;

        public PixelFetcher(MemoryMapper mmu)
        {
            _mmu = mmu;
        }
    }
}
