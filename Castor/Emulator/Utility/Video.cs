using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public static class Video
    {
        public static byte GetGrayShade(int colorValue, byte reg)
        {
            int i = reg.BitValue(colorValue * 2 + 1) << 1 |
                reg.BitValue(colorValue * 2);

            return (byte)GetColor(i);
        }

        public static ColorDepthValue GetColor(int idx)
        {
            if (idx == 0)
                return ColorDepthValue.White;
            if (idx == 1)
                return ColorDepthValue.LightGray;
            if (idx == 2)
                return ColorDepthValue.DarkGray;
            if (idx == 3)
                return ColorDepthValue.Black;

            throw new Exception("Color not valid.");
        }

        public enum ColorDepthValue : byte
        {
            White = 255,
            LightGray = 196,
            DarkGray = 97,
            Black = 0
        }
    }
}
