using Castor.Emulator.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public static class Pallette
    {
        public static ColorPallette GetColor(byte palletteReg, int idx)
        {
            if (idx > 3 || idx < 0)
                throw new Exception("Pallette color out of bounds.");

            return (ColorPallette)(palletteReg.BitValue(idx * 2) << 1 | palletteReg.BitValue(idx * 2 - 1));
        }
    }
}
