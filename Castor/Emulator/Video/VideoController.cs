using Castor.Emulator.Utility;
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

        private byte _stat;
        private byte _lcdc;
        private byte _bgp;

        private int _mode;
        private int _modeclock;
        private int _line;

        private GameboySystem _system;

        public byte STAT
        {
            get => _stat;
            set => _stat = value;
        }

        public byte LCDC
        {
            get => _lcdc;
            set => _lcdc = value;
        }

        public byte BGP
        {
            get =>  _bgp;
            set => _bgp = value;
        }

        public byte LY
        {
            get => (byte)_line;
            set => _line = 0;
        }

        public VideoController(GameboySystem system)
        {
            _vram = new byte[0x2000];
            _oam = new byte[0xA0];
            _system = system;
            _line = 0;
            _mode = 0;
            _modeclock = 0;
        }

        public byte this[int idx]
        {
            get
            {
                if (idx >= 0x8000 && idx < 0xA000)
                {
                    return _vram[idx - 0x8000];
                }
                else if (idx >= 0xFE00 && idx < 0xFEA0)
                {
                    return _oam[idx - 0xFE00];
                }

                throw new Exception("This memory address should not be mapped to this component.");
            }
            set
            {
                if (idx >= 0x8000 && idx < 0xA000)
                {
                    _vram[idx - 0x8000] = value;
                }
                else if (idx >= 0xFE00 && idx < 0xFEA0)
                {
                    _oam[idx - 0xFE00] = value;
                }
            }
        }

        private void RenderScanline()
        {

        }

        public void Step()
        {
            // check if lcd is even enabled, if not return
            if (!_lcdc.CheckBit(BitFlags.Bit7))
                return;

            ++_modeclock;

            switch (_mode)
            {
                // OAM read
                case 2:
                    if (_modeclock >= 80) // about 77 - 83 clocks (avg = 80)
                    {
                        _modeclock = 0;
                        _mode = 3;
                    }
                    break;

                // Pixel transfer mode
                case 3:
                    if (_modeclock >= 172) // about 169 - 175 clocks (avg = 172)
                    {
                        _modeclock = 0;
                        _mode = 0; // next hblank mode so techinically this is end of horiz line

                        // Time to render scan line
                        RenderScanline();
                    }
                    break;

                // Hblank period
                case 0:
                    if (_modeclock >= 204) // about 201 - 207 clocks (avg = 204)
                    {
                        _modeclock = 0;
                        _line++; // end of hblank period means new line

                        if (_line == 143) // if this is last line then enter vblank
                        {
                            _mode = 1;
                            // TODO: render framebuffer as this end of the frame
                        }
                        else // otherwise just ender OAM read
                        {
                            _mode = 2;
                        }
                    }
                    break;

                // Vblank period
                case 1:
                    {
                        if (_modeclock >= 456)
                        {
                            _modeclock = 0;
                            _line++; // still need to increment lines for LY reg
                        }

                        if (_line > 153) // lasts 4560 clocks
                        {
                            _modeclock = 0;
                            _line = 0; // reset line
                            _mode = 2; // reenter oam read
                        }
                    }
                    break;
            }
        }
    }
}
