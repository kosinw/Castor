using Castor.Emulator.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public partial class VideoController : IAddressableComponent
    {
        #region Private Members        
        private enum RenderMode : byte
        {
            HBlank = 0,
            PixelTransfer = 1,
            OAMRead = 2,
            VBlank = 3
        }

        // GPU Data Stuff
        private byte[] _vram;
        private byte[] _oam;
        private Color[,] _framebuffer;

        // Register stuff
        private BitFlags _stat;
        private BitFlags _lcdc;

        private byte _bgp;
        private byte _scx;
        private byte _scy;
        private byte _obp0;
        private byte _obp1;

        // Timing stuff
        private RenderMode _mode;
        private int _modeclock;
        private int _line;

        private GameboySystem _system;
        #endregion

        #region IO Registers
        public byte STAT
        {
            get
            {
                return (byte)(((byte)_stat >> 2) << 2 | (byte)_mode); // make sure the last two bits are clear
            }

            set
            {
                byte realValue = (byte)((value >> 2) << 2);
                _stat = (BitFlags)value;
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
            get => (byte)_lcdc;
            set => _lcdc = (BitFlags)value;
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
            get => _obp0;
            set => _obp0 = value;
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
            _mode = RenderMode.OAMRead;
            _modeclock = 0;
            _framebuffer = new Color[160, 144];
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
            if (_lcdc.HasFlag(BitFlags.Bit0))
                RenderBackground();
        }

        private void RenderBackground()
        {
            // Find Background Tile Data
            bool tileDataDisplaySelect = _lcdc.HasFlag(BitFlags.Bit4);
            ushort tileDataZero = tileDataDisplaySelect ? (ushort)0x8000 : (ushort)0x8800;

            // Find Background Tile Map
            bool tileMapDisplaySelect = _lcdc.HasFlag(BitFlags.Bit3);
            ushort tileMapZero = tileMapDisplaySelect ? (ushort)0x9C00 : (ushort)0x9800;

            // Check if the tile map is numbered from 0 to 255 or -127 to 128
            bool isSigned = !tileMapDisplaySelect;

            int y = _line + _scy;

            // for each tile in this line
            for (int tile = 0; tile < 20; ++tile)
            {
                // 32x32 bytes for tilemap, 8x8 pixels in one tile
                int tileIndex = tileMapZero + (y / 8) + tile;
                sbyte tileID = (sbyte)_system.MMU[tileIndex];

                if (isSigned)
                {
                    tileID += 127;
                }

                byte lsbLine = _system.MMU[tileDataZero + (tileID * 16) + (y % 8)];
                byte msbLine = _system.MMU[tileDataZero + (tileID * 16) + (y % 8) + 1];

                // For each pixel
                for (int x = 0; x < 8; ++x)
                {
                    
                }
            }
        }

        public void Step()
        {
            // check if lcd is even enabled, if not return
            if ((_lcdc & BitFlags.Bit7) == BitFlags.Bit7)
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
