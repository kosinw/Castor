using Castor.Emulator.Memory;
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
    public partial class VideoController
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
        private byte _lyc;

        // Timing stuff
        private int _mode;
        private int _modeclock;
        private int _line;

        // Window stuff
        private int _wx;
        private int _wy;

        private Device _d;
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

        public byte LCDC
        {
            get => _lcdc;
            set
            {
                if (Bit.BitValue(value, 7) == 0)
                {
                    _line = 0;
                    _mode = 0;
                    _modeclock = 0;
                }

                _lcdc = value;
                _mode = 2;
            }
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
        }

        public byte LYC
        {
            get => _lyc;
            set => _lyc = value;
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

        public byte WX
        {
            get => (byte)(_wx + 7);
            set => _wx = value - 7;
        }

        public byte WY
        {
            get => (byte)_wy;
            set => _wy = value;
        }
        #endregion

        #region Constructors
        public VideoController(Device system)
        {
            _vram = new byte[0x2000];
            _oam = new byte[0xA0];
            _d = system;
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
            if (Bit.BitValue(_lcdc, 0) == 1)
                RenderBackground();
            else
                Array.Clear(_framebuffer, 0, _framebuffer.Length);

            if (Bit.BitValue(_lcdc, 1) == 1)
                RenderSprites();
        }

        private void RenderSprites()
        {
        }

        private void RenderBackground()
        {
            // Here check which data set is being used
            // For 0x8000 the tile indices are numbered from 0 to 255
            // For 0x8800 the tile indices are numbered from -128 to 127
            var tileDataZero = Bit.BitValue(_lcdc,4) == 1 ? 0x8000 : 0x8800;
            var usingSignedIndices = Bit.BitValue(_lcdc, 4) == 0;

            // Check if the window is enabled, if so then calculate when the wx and wy start
            var usingWindow = false;

            // If the window is on screen and the window y is greater than the scroll y register
            // enable using windows
            if (Bit.BitValue(_lcdc, 5) == 1 && _wy > _scy)
            {
                usingWindow = true;
            }

            // Here check which map set contains the background tile map
            var tileMapZero = Bit.BitValue(_lcdc, 3) == 0 ? 0x9800 : 0x9C00;            

            // This is used to calculate which row of 32 tiles the GPU is currently on
            var yPosition = (_scy + _line) % 256;

            if (usingWindow)
            {
                tileMapZero = Bit.BitValue(_lcdc, 6) == 0 ? 0x9800 : 0x9C00;
                yPosition = (_scy - _wy) % 256;
            }

            // This is used to calculate which line of whatever tile the GPU is rendering
            var tileRow = (yPosition / 8) * 32;
            
            // For each pixel on this scanline
            for (int x = 0; x < 160; ++x)
            {
                var xPosition = (x + _scx) % 256;

                if (usingWindow && x >= _wx)
                {
                    xPosition = x - _wx;
                }

                var tileColumn = (xPosition / 8) % 256;
                int tileAddress = tileMapZero + tileRow + tileColumn;

                // Here find the index of the tile that will be rendered
                int tileIdx = 0;
                if (usingSignedIndices)
                    tileIdx = (sbyte)_d.MMU[tileAddress];
                else
                    tileIdx = (byte)_d.MMU[tileAddress];

                // Here find start location of the tile by index
                int tileLocation = 0;

                if (usingSignedIndices)
                    tileLocation += (tileIdx + 128) * 16;
                else
                    tileLocation += tileIdx * 16;

                int vline = yPosition % 8; // Each tile is 8 pixels high
                vline *= 2; // Each line takes up two bytes

                byte b1 = _d.MMU[tileDataZero + tileLocation + vline]; // read the first half of the line
                byte b2 = _d.MMU[tileDataZero + tileLocation + vline + 1]; // read the second half of the line

                int currentBit = 7 - (xPosition % 8); // the most significant bit is the first bit rendered
                int colorValue = 2 * Bit.BitValue(b1, currentBit) + Bit.BitValue(b2, currentBit);

                _framebuffer[_line * 160 + x] = Utility.Video.GetGrayShade(colorValue, _bgp);
            }
        }

        public void Step(int cycles)
        {
            // check if lcd is even enabled, if not return
            if (Bit.BitValue(_lcdc, 7) == 0)
                return;

            // increment modeclock, used to switch between gpu modes
            _modeclock += cycles;

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

                        // handle h-blank interrupt if stat bit 3
                        if (Bit.BitValue(_stat, 3) == 1)
                            _d.ISR.EnableInterrupt(InterruptFlags.LCDStat);
                    }
                    break;

                // Hblank period
                case 0:
                    if (_modeclock >= 204) // about 201 - 207 clocks (avg = 204)
                    {
                        _modeclock = 0;
                        _line++; // end of hblank period means new line

                        // do ly coincidence
                        if (_line == _lyc)
                        {
                            // trigger lcd stat interrupt if bit 6 is set
                            if (Bit.BitValue(_stat, 6) == 1)
                                _d.ISR.EnableInterrupt(InterruptFlags.LCDStat);

                            Bit.SetBit(_stat, 2);
                        }
                        // otherwise unset ly coincidence flag
                        else
                            Bit.ClearBit(_stat, 2);

                        if (_line == 143) // if this is last line then enter vblank
                        {
                            _mode = 1;
                            OnRenderEvent();

                            _d.ISR.EnableInterrupt(InterruptFlags.VBlank);

                            // trigger another interrupt if stat bit 3 is set
                            if (Bit.BitValue(_stat, 4) == 1)
                                _d.ISR.EnableInterrupt(InterruptFlags.LCDStat);
                        }
                        else // otherwise just enter OAM read
                        {
                            _mode = 2;

                            // handle oam interrupt if stat bit 5 is set
                            if (Bit.BitValue(_stat, 5) == 1)
                                _d.ISR.EnableInterrupt(InterruptFlags.LCDStat);
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

                            // handle oam interrupt if stat bit 5 is set
                            if (Bit.BitValue(_stat, 5) == 1)
                                _d.ISR.EnableInterrupt(InterruptFlags.LCDStat);
                        }
                    }
                    break;
            }
        }
    }
}
