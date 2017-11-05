using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public struct PixelMetadata
    {
        public enum PriorityMode : int
        {
            None,
            Background,
            Sprite
        }

        public PriorityMode Priority;
        public byte? SpritePriority;
        public byte PalletteData;
    }
}
