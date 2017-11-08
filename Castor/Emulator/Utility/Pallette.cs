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

        public static byte[] ToRGB(ColorPallette color)
        {
            switch (color)
            {
                case ColorPallette.Black:
                    return new byte[] { 0, 0, 0 };
                case ColorPallette.White:
                    return new byte[] { 255, 255, 255 };
                case ColorPallette.LightGray:
                    return new byte[] { 127, 127, 127 };
                default:
                    return new byte[] { 96, 96, 96 };
            }
        }
    }
}
