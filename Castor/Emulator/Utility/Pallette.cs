using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public static class Pallette
    {
        public static byte GetColor(int idx, byte reg)
        {
            switch (reg.BitValue(idx * 2 + 1) << 1 | reg.BitValue(idx * 2))
            {
                case 0: return 255;
                case 1: return 197;
                case 2: return 96;
                default: return 0;
            }
        }
    }
}
