using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public partial class VideoController : IAddressableComponent
    {
        private byte[] _vram;
        private byte[] _oam;

        private GameboySystem _system;        

        public VideoController(GameboySystem system)
        {
            _vram = new byte[0x2000];
            _oam = new byte[0xA0];
            _system = system;
        }

        public byte this[int idx]
        {
            get
            {
                if (idx >= 0x8000 && idx < 0xA000)
                    return _vram[idx - 0x8000];
                else if (idx >= 0xFE00 && idx < 0xFEA0)
                    return _oam[idx - 0xFE00];

                throw new Exception("This memory address should not be mapped to this component.");
            }
            set
            {
                if (idx >= 0x8000 && idx < 0xA000)
                    _vram[idx - 0x8000] = value;
                else if (idx >= 0xFE00 && idx < 0xFEA0)
                    _oam[idx - 0xFE00] = value;
            }
        }
    }
}
