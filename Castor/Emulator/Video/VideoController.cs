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
        private enum RenderMode : byte
        {
            HBlank = 0,
            PixelTransfer = 1,
            OAMRead = 2,
            VBlank = 3
        }

        private byte[] _vram;
        private byte[] _oam;        
        private PalletteData[,] _screenbuffer;

        private PalletteData[] _bgPallette;
        private PalletteData[] _obPallette0;
        private PalletteData[] _obPallette1;

        private byte _stat;
        private byte _lcdc;
        private byte _bgp;
        private byte _scx;
        private byte _scy;
        private byte _obp0;
        private byte _obp1;

        private RenderMode _mode;
        private int _modeclock;
        private int _line;

        private PixelFIFO _fifo;
        private PixelFetcher _fetcher;

        private GameboySystem _system;

        public byte STAT
        {
            get
            {                
                return (byte)((_stat >> 2) << 2 | (byte)_mode); // make sure the last two bits are clear
            }

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
            set
            {
                _bgp = value;
                _bgPallette = value.ToPallette();
            }
        }

        public byte OBP0
        {
            get => _obp0;
            set
            {
                _obp0 = value;
                _obPallette0 = value.ToPallette();
            }
        }

        public byte OBP1
        {
            get => _obp1;
            set
            {
                _obp1 = value;
                _obPallette0 = value.ToPallette();
            }
        }

        public byte LY
        {
            get => (byte)_line;
            set => _line = 0;
        }

        public byte SCX
        {
            get => _scx;
            set => _scx = value;
        }

        public byte SCY
        {
            get => _scy;
            set => _scy = value;
        }

        public VideoController(GameboySystem system)
        {
            _vram = new byte[0x2000];
            _oam = new byte[0xA0];
            _system = system;
            _line = 0;
            _mode = RenderMode.OAMRead;
            _modeclock = 0;
            _screenbuffer = new PalletteData[160, 144]; // the resolution of GBC is 160x144

            _fifo = new PixelFIFO();
            _fetcher = new PixelFetcher(_system.MMU);
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

        private bool IsLCDEnabled() => LCDC.CheckBit(BitFlags.Bit7);
        private bool WindowTileMapDisplay() => LCDC.CheckBit(BitFlags.Bit6);
        private bool IsWindowEnabled() => LCDC.CheckBit(BitFlags.Bit5);
        private bool ShouldShowSprites() => LCDC.CheckBit(BitFlags.Bit1);
        private bool IsBackgroundEnabled() => LCDC.CheckBit(BitFlags.Bit0);

        private void RenderScanline()
        {
            int xPixelsToRemove = _scx;
            int currentPosition = 0;

            while (true)
            {
                if (xPixelsToRemove > 0)
                {
                    --xPixelsToRemove;
                    continue;
                }

                if (currentPosition >= 160) // for 160 pixels
                    break;

                if (_fifo.Enabled) // here is the dequeing routine
                {
                    PixelMetadata pixel_metadata = _fifo.Dequeue();
                   
                    // set the current x, y position to the color specified in the pallette
                    _screenbuffer[currentPosition, _line] = _bgPallette[pixel_metadata.PalletteData];
                }
            }
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
                case RenderMode.OAMRead:
                    if (_modeclock >= 80) // about 77 - 83 clocks (avg = 80)
                    {
                        _modeclock = 0;
                        _mode = RenderMode.PixelTransfer;
                    }
                    break;

                // Pixel transfer mode
                case RenderMode.PixelTransfer:
                    if (_modeclock >= 172) // about 169 - 175 clocks (avg = 172)
                    {
                        _modeclock = 0;
                        _mode = RenderMode.HBlank; // next hblank mode so techinically this is end of horiz line

                        RenderScanline();
                    }
                    break;

                // Hblank period
                case RenderMode.HBlank:
                    if (_modeclock >= 204) // about 201 - 207 clocks (avg = 204)
                    {
                        _modeclock = 0;
                        _line++; // end of hblank period means new line

                        if (_line == 143) // if this is last line then enter vblank
                        {
                            _mode = RenderMode.VBlank;
                            // TODO: render framebuffer as this end of the frame
                        }
                        else // otherwise just enter OAM read
                        {
                            _mode = RenderMode.OAMRead;
                        }
                    }
                    break;

                // Vblank period
                case RenderMode.VBlank:
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
                            _mode = RenderMode.OAMRead; // reenter oam read
                        }
                    }
                    break;
            }
        }
    }
}
