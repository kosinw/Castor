using Castor.Emulator.Memory;
using Castor.Emulator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public class Tile
    {
        public const int TILE_WIDTH_PX = 8;
        public const int TILE_HEIGHT_PX = 8;

        private byte[] _buffer;

        public Tile(MemoryMapper mmu, ushort address)
        {
            _buffer = new byte[TILE_WIDTH_PX * TILE_HEIGHT_PX];

            for (int tileLine = 0; tileLine < TILE_HEIGHT_PX; ++tileLine)
            {
                int tileIndex = 2 * tileLine; // need to multiply by two as each line is 2 bytes
                int tileLineAddress = tileIndex + address; // address is offset to index

                byte lsbPixels = mmu[tileLineAddress];
                byte msbPixels = mmu[tileLineAddress + 1];

                byte[] pixels = GetPixelsInLine(lsbPixels, msbPixels);

                for (int x = 0; x < 8; ++x)
                {
                    _buffer[GetIndex(x, tileLine)] = pixels[x];
                }
            }
        }

        public byte GetPixel(int x, int y) => _buffer[GetIndex(x, y)];

        private byte[] GetPixelsInLine(byte lsb, byte msb)
        {
            byte[] buf = new byte[8];

            for (int i = 0; i < 8; ++i)
            {
                byte colorValue = (byte)(msb.BitValue(7 - i) << 1 | lsb.BitValue(7 - i));
                buf[i] = colorValue;
            }

            return buf;
        }

        private int GetIndex(int x, int y)
        {
            return (TILE_HEIGHT_PX * y) + x;
        }
    }
}
