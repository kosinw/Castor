using Castor.Emulator.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public class Tile
    {
        const int TILE_WIDTH_PX = 8;
        const int TILE_HEIGHT_PX = 8;        

        private ColorPallette[] _buffer;

        public Tile(MemoryMapper mmu, ushort address)
        {
            _buffer = new ColorPallette[TILE_WIDTH_PX * TILE_HEIGHT_PX];            

            for (int y = 0; y < TILE_HEIGHT_PX; ++y)
            {
                (byte, byte) line = GetTileLine(y, mmu, address);

                for (int x = 0; x < TILE_WIDTH_PX; ++x)
                {
                    
                }
            }
        }

        public (byte, byte) GetTileLine(int lineNumber, MemoryMapper mmu, ushort address)
        {
            return (0, 0);
        }
    }
}
