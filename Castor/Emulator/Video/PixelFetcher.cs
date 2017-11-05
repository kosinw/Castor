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
        public enum SearchingMode
        {
            Background,
            Windows,
            Sprites
        }

        private int _spritePriorityCounter;

        private GameboySystem _system;

        public bool Enabled;
        public SearchingMode Search;

        public PixelFetcher(GameboySystem system)
        {
            _system = system;
            _spritePriorityCounter = 0;
            Search = SearchingMode.Background;
            Enabled = true;
        }

        // Fetch the whole scanline
        public PixelMetadata[] FetchLine(int x, int y)
        {
            if (Search == SearchingMode.Background)
            {
            }
            else if (Search == SearchingMode.Sprites)
            {
                _spritePriorityCounter++;
            }

            return Array.Empty<PixelMetadata>();
        }
    }
}
