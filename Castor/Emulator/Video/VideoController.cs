using Castor.Emulator.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public partial class VideoController : IAddressableComponent
    {
        public delegate void RenderEventHandler();
        public event RenderEventHandler OnRenderEvent;

        public const int RENDER_WIDTH = 160;
        public const int RENDER_HEIGHT = 144;

        public IntPtr Screen
        {
            get
            {
                var handle = GCHandle.Alloc(_framebuffer, GCHandleType.Pinned);
                var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(_framebuffer, 0);
                return ptr;
            }
        }

        #region Private Members 
        // GPU Data Stuff
        private byte[] _vram;
        private byte[] _oam;
        private byte[] _framebuffer;

        // Register stuff
        private byte _stat;
        private byte _lcdc;

        private byte _bgp;
        private byte _scx;
        private byte _scy;
        private byte _obp0;
        private byte _obp1;

        // Timing stuff
        private int _mode;
        private int _modeclock;
        private int _line;

        private GameboySystem _system;
        #endregion

        #region IO Registers
        public byte STAT
        {
            get
            {
                return (byte)((_stat >> 2) << 2 | _mode); // make sure the last two bits are clear
            }

            set
            {
                byte realValue = (byte)((value >> 2) << 2);
                _stat = value;
            }
        }

        //FF40 - LCDC - LCD Control(R/W)
        //    Bit 7 - LCD Display Enable(0=Off, 1=On)
        //    Bit 6 - Window Tile Map Display Select(0=9800-9BFF, 1=9C00-9FFF)
        //    Bit 5 - Window Display Enable(0=Off, 1=On)
        //    Bit 4 - BG & Window Tile Data Select(0=8800-97FF, 1=8000-8FFF)
        //    Bit 3 - BG Tile Map Display Select(0=9800-9BFF, 1=9C00-9FFF)
        //    Bit 2 - OBJ(Sprite) Size(0=8x8, 1=8x16)
        //    Bit 1 - OBJ(Sprite) Display Enable(0=Off, 1=On)
        //    Bit 0 - BG Display(for CGB see below) (0=Off, 1=On)
        public byte LCDC
        {
            get => _lcdc;
            set => _lcdc = value;
        }

        public byte BGP
        {
            get => _bgp;
            set => _bgp = value;
        }

        public byte OBP0
        {
            get => _obp0;
            set => _obp0 = value;
        }

        public byte OBP1
        {
            get => _obp1;
            set => _obp1 = value;
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
        #endregion

        #region Constructors
        public VideoController(GameboySystem system)
        {
            _vram = new byte[0x2000];
            _oam = new byte[0xA0];
            _system = system;
            _line = 0;
            _mode = 2;
            _modeclock = 0;
            _framebuffer = new byte[160 * 144];
        }
        #endregion

        #region Memory Mapping
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
        #endregion

        private void RenderScanline()
        {
            // Check if Bit0 (BG Display Flag) is set
            if (_lcdc.BitValue(0) == 1)
                RenderBackground();
        }

        private void RenderBackground()
        {
            // First check which tile map is being used
            int tileMapOffset = _lcdc.BitValue(3) == 0 ?
                0x9800 : 0x9C00;

            // Then check which tile data set is being used
            int tileDataOffset = _lcdc.BitValue(4) == 0 ?
                0x8800 : 0x8000;

            // Check if tile data set is numbered starting with -128
            bool usingSignedIndices = _lcdc.BitValue(4) == 0;

            // Check which row of the tile map is going to be rendered
            int tileRow = ((_line + _scy) % 256) / 8;

            // Check the initial column of the tile map is going to be rendered
            int tileCol = _scx / 8;

            // Find line of the tile will be rendered
            int yIndex = (_scy + _line) % 8;

            // Find the initial x index in tileline to start at
            int xIndex = _scx % 8;

            // The initial tile based off of the index
            short tileIndex = (sbyte)_system.MMU[(tileRow * 32) + tileCol + tileMapOffset];

            // If signed indices are being used, add an offset of 128
            if (usingSignedIndices) tileIndex += 128;

            // Otherwise make sure values greater than 127 are their signed counterparts
            else tileIndex = (byte)tileIndex;

            // For every pixel on this raster line
            for (int x = 0; x < 160; ++x)
            {
                byte byte1 = _system.MMU[tileDataOffset + (16 * tileIndex) + (yIndex * 2)];
                byte byte2 = _system.MMU[tileDataOffset + (16 * tileIndex) + (yIndex * 2) + 1];

                int idx = byte1.BitValue(7 - xIndex) << 1 | byte2.BitValue(7 - xIndex);

                _framebuffer[_line * 160 + x] = Pallette.GetColor(idx, _bgp);

                xIndex++;

                // If the tile has completed, move onto the next tile
                if (xIndex == 8)
                {
                    xIndex = 0;
                    tileCol = (tileCol + 1) % 32;

                    tileIndex = (sbyte)_system.MMU[tileRow * 32 + tileCol + tileMapOffset];

                    if (usingSignedIndices) tileIndex += 128;
                    else tileIndex = (byte)tileIndex;
                }
            }
        }

        public void Step()
        {
            // check if lcd is even enabled, if not return
            if (_lcdc.BitValue(7) == 0)
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
                            OnRenderEvent();
                        }
                        else // otherwise just enter OAM read
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
