using Castor.Emulator.Memory;
using Castor.Emulator.Utility;
using System;

namespace Castor.Emulator.Video
{
    public partial class VideoController
    {
        // Public Constants
        public const float RENDER_WIDTH = 160.0f;
        public const float RENDER_HEIGHT = 144.0f;

        // Pallete Constants
        private const int White = 255;
        private const int LightGray = 196;
        private const int DarkGray = 97;
        private const int Black = 0;
        private const int Transparent = 1;

        /// <summary>
        /// Fetches the VideoController's generated screen buffer.
        /// </summary>
        /// <returns>A texture of the framebuffer in RGBA24 format.</returns>
        public byte[] GetScreenBuffer()
        {
            byte[] framebuffer = new byte[_framebuffer.Length * 4];

            for (int i = 0; i < _framebuffer.Length; ++i)
            {
                framebuffer[i * 4 + 0] = _framebuffer[i];
                framebuffer[i * 4 + 1] = _framebuffer[i];
                framebuffer[i * 4 + 2] = _framebuffer[i];
            }

            return framebuffer;
        }

        #region Private Members 
        // GPU Data Stuff
        private byte[] _vram;
        private byte[] _oam;
        private byte[] _framebuffer;        

        // Pallete Data
        private byte[] _bgpallete = new byte[] { White, LightGray, DarkGray, Black };
        private byte[] _objpallete0 = new byte[] { Transparent, LightGray, DarkGray, Black };
        private byte[] _objpallete1 = new byte[] { Transparent, LightGray, DarkGray, Black };

        // Lcd control and status registers
        private byte _stat;
        private byte _lcdc;

        // Lcd line registers
        private byte _scx;
        private byte _scy;
        private byte _lyc;

        // Lcdc timing sstuff
        private int _mode;
        private int _modeclock;
        private int _line;

        // Window stuff
        private int _wx;
        private int _wy;

        private Device _d;
        private bool _vblankTriggered = false;
        #endregion

        #region IO Registers
        public byte STAT
        {
            get
            {
                return (byte)((_stat >> 2) << 2 | _mode); // sanity check of last two stat bits
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
                if (Bit.BitValue(value, 0) == 0)
                {
                    Array.Clear(_framebuffer, 0, _framebuffer.Length);
                }

                _lcdc = value;
            }
        }

        public byte BGP
        {
            get
            {
                return (byte)(
                    IndexFromShade(_bgpallete[3]) << 6 |
                    IndexFromShade(_bgpallete[2]) << 4 |
                    IndexFromShade(_bgpallete[1]) << 2 |
                    IndexFromShade(_bgpallete[0]) << 0);
            }

            set
            {
                _bgpallete[3] = (byte)ShadeFromIndex((value >> 6) & 3);
                _bgpallete[2] = (byte)ShadeFromIndex((value >> 4) & 3);
                _bgpallete[1] = (byte)ShadeFromIndex((value >> 2) & 3);
                _bgpallete[0] = (byte)ShadeFromIndex((value >> 0) & 3);
            }
        }

        public byte OBP0
        {
            get
            {
                return (byte)(
                    IndexFromShade(_objpallete0[3]) << 6 |
                    IndexFromShade(_objpallete0[2]) << 4 |
                    IndexFromShade(_objpallete0[1]) << 2);
            }

            set
            {
                _objpallete0[3] = (byte)ShadeFromIndex((value >> 6) & 3);
                _objpallete0[2] = (byte)ShadeFromIndex((value >> 4) & 3);
                _objpallete0[1] = (byte)ShadeFromIndex((value >> 2) & 3);
            }
        }

        public byte OBP1
        {
            get
            {
                return (byte)(
                    IndexFromShade(_objpallete1[3]) << 6 |
                    IndexFromShade(_objpallete1[2]) << 4 |
                    IndexFromShade(_objpallete1[1]) << 2);
            }

            set
            {
                _objpallete1[3] = (byte)ShadeFromIndex((value >> 6) & 3);
                _objpallete1[2] = (byte)ShadeFromIndex((value >> 4) & 3);
                _objpallete1[1] = (byte)ShadeFromIndex((value >> 2) & 3);
            }
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

        /// <summary>
        /// Writes a line to the framebuffer.
        /// </summary>
        private void RenderScanline()
        {
            // Check if Bit0 (BG Display Flag) is set
            if (Bit.BitValue(_lcdc, 0) == 1)
            {
                RenderBackground();
            }

            // Check if Bit1 (OBJ Display Flag) is set
            if (Bit.BitValue(_lcdc, 1) == 1)
            {
                RenderSprites();
            }
        }

        /// <summary>
        /// Renders the background portion to the current scanline in the framebuffer.
        /// </summary>
        private void RenderBackground()
        {
            if (_line >= 144) // sanity check
            {
                return;
            }

            // Here check which data set is being used
            // For 0x8000 the tile indices are numbered from 0 to 255
            // For 0x8800 the tile indices are numbered from -128 to 127
            var tileDataZero = Bit.BitValue(_lcdc, 4) == 1 ? 0x8000 : 0x8800;
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
                {
                    tileIdx = (sbyte)_d.MMU[tileAddress];
                }

                else
                {
                    tileIdx = _d.MMU[tileAddress];
                }

                // Here find start location of the tile by index
                int tileLocation = 0;

                if (usingSignedIndices)
                    tileLocation += (tileIdx + 128) * 16;
                else
                    tileLocation += tileIdx * 16;

                int vline = yPosition % 8; // Each tile is 8 pixels high
                vline *= 2; // Each line takes up two /*bytes*/

                byte b1 = _d.MMU[tileDataZero + tileLocation + vline]; // read the first half of the line
                byte b2 = _d.MMU[tileDataZero + tileLocation + vline + 1]; // read the second half of the line

                int currentBit = 7 - (xPosition % 8); // the most significant bit is the first bit rendered
                int colorValue = 2 * Bit.BitValue(b1, currentBit) + Bit.BitValue(b2, currentBit);

                _framebuffer[_line * 160 + x] = _bgpallete[colorValue];
            }
        }

        /// <summary>
        /// Writes spritedata for current scanline into framebuffer.
        /// </summary>
        private void RenderSprites()
        {
            if (_line >= 144) // sanity check
            {
                return;
            }

            // Foreach object attribute entry in OAM RAM and while renderedSprites < 10:
            // Check if the object's y position is on the current scanline
            // Check how much of the sprite is visible on the scanline
            // Calculate sprite effects such as x and y reflections
            // Find tiles in VRAM corresponding to x and y position
            // Calculate what the sprites color would be either using OBP0 or OBP1
            // Calculate sprite priority by using these rules:
            //      - If a (x,y) pair's color value in vram = 0, then the sprite should not be displayed
            //      - If a (x,y) pair overlaps with a different sprite then:
            //          - If the x value of the (x,y) pair is less then override the sprite
            //          - otherwise if they are on top of each other then the first one in position should not be overrided
            //          - otherwise don't override the already in position sprite
            //      - If a sprite's priority = 1 and background color > 0, then the sprite should not be displayed
            // Then overwrite the pixels in the framebuffer with the sprites

            int numSprites = 0;
            byte[] spriteScanline = new byte[160];

            for (int i = 0; i < spriteScanline.Length; ++i)
            {
                spriteScanline[i] = Transparent;
            }

            for (int i = 0; i < 40 && numSprites < 10; ++i)
            {
                // There are 4 bytes for each oam entry
                var yPosition = _oam[i * 4 + 0] - 16;
                var xPosition = _oam[i * 4 + 1] - 8;
                var tileIndex = _oam[i * 4 + 2];
                var flags = _oam[i * 4 + 3];

                // If Bit2 LCDC is set = 16, otherwise 8
                var height = Bit.BitValue(flags, 2) == 0 ? 8 : 16;

                // Check if object's y position is on the current scanline
                var top = yPosition;
                var bottom = yPosition + height;

                if (top <= _line && bottom > _line)
                {
                    // Calculate vertical line to read pixels from
                    bool verticalMirror = Bit.BitValue(flags, 6) == 1;
                    int vline = _line - top;

                    if (verticalMirror)
                    {
                        vline -= height;    // if vline = 1 then vline -= 8 will give -7 (which is its additive inverse equivalent mirrored)
                        vline *= -1;        // additive inverse so its sign is correct
                    }

                    vline *= 2;

                    // Sprite Tile Data is stored in memory the same binary format as Background tiles
                    byte b1 = _vram[tileIndex * 16 + vline + 0];
                    byte b2 = _vram[tileIndex * 16 + vline + 1];

                    bool horizontalMirror = Bit.BitValue(flags, 5) == 1;
                    bool aboveBackground = Bit.BitValue(flags, 7) == 0;
                    byte[] objectPallete = Bit.BitValue(flags, 4) == 0 ? _objpallete0 : _objpallete1;

                    // Calculate horizontal lines to read pixels from
                    for (int x = 7; x >= 0; --x)
                    {
                        if (horizontalMirror)
                        {
                            x -= 8;
                            x *= -1;
                        }

                        var currentBit = 7 - x;
                        var color = 2 * Bit.BitValue(b1, currentBit) + Bit.BitValue(b2, currentBit);

                        // Calculate sprite priority
                        if (color == Transparent)
                        {
                            continue;
                        }

                        if (xPosition + x < 160 && xPosition + x > 0)
                        {
                            spriteScanline[xPosition + x] = objectPallete[color];
                        }                        
                    }

                    numSprites++; // this sprite is on this line
                }
            }

            // Copy all the sprite data onto the background
            // The reason why data is written elsewhere is to calculate sprite prio

            for (int i = 0; i < spriteScanline.Length; ++i)
            {
                if (spriteScanline[i] != Transparent)
                {
                    _framebuffer[160 * _line + i] = spriteScanline[i];
                }
            }
        }

        /// <summary>
        /// Converts a gray shade (gray8) value into either 0 (White), 1 (LightGray), 2 (DarkGray), or 3 (Black).
        /// </summary>
        /// <param name="shade">Gray8 color value</param>
        /// <returns>0 if White, 1 if LightGray, 2 if DarkGray, 3 if Black</returns>
        private int IndexFromShade(int shade)
        {
            if (shade == White)
            {
                return 0;
            }

            else if (shade == LightGray)
            {
                return 1;
            }

            else if (shade == DarkGray)
            {
                return 2;
            }

            else if (shade == Black)
            {
                return 3;
            }

            return 0;
        }

        /// <summary>
        /// Converts 0 (White), 1 (LightGray), 2 (DarkGray), 3 (Black) into their respective Gray8 values.
        /// </summary>
        /// <param name="idx">0 (White), 1 (LightGray), 2 (DarkGray), or 3 (Black)</param>
        /// <returns>Shade of Gray8</returns>
        private int ShadeFromIndex(int idx)
        {
            if (idx == 0)
            {
                return White;
            }

            else if (idx == 1)
            {
                return LightGray;
            }

            else if (idx == 2)
            {
                return DarkGray;
            }

            else if (idx == 3)
            {
                return Black;
            }

            return 0;
        }

        /// <summary>
        /// Emulates a certain number of cycles on the LCD.
        /// </summary>
        /// <param name="cycles">Number of CPU clocks to emulate</param>
        public void Step(int cycles)
        {
            // check if lcd is even enabled, if not return
            if (Bit.BitValue(_lcdc, 7) == 0)
            {
                return;
            }

            // increment modeclock, used to switch between gpu modes
            _modeclock += cycles;

            switch (_mode)
            {
                // OAM read
                case 2:
                    if (_modeclock >= 80) // about 77 - 83 clocks (avg = 80)
                    {
                        _modeclock -= 80;
                        _mode = 3;
                    }
                    break;

                // Pixel transfer mode
                case 3:
                    if (_modeclock >= 172) // about 169 - 175 clocks (avg = 172)
                    {
                        _modeclock -= 172;
                        _mode = 0; // next hblank mode so techinically this is end of horiz line

                        RenderScanline();
                    }
                    break;

                // Hblank period
                case 0:
                    if (_modeclock >= 204) // about 201 - 207 clocks (avg = 204)
                    {
                        _modeclock -= 204;
                        _line++; // end of hblank period means new line

                        if (_line == 143) // if this is last line then enter vblank
                        {
                            _mode = 1;
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
                            _modeclock -= 456;
                            _line++; // still need to increment lines for LY reg
                        }

                        if (_line == 144 && !_vblankTriggered)
                        {
                            _d.IRQ.RequestInterrupt(InterruptFlags.VBL);
                            _vblankTriggered = true;
                        }

                        if (_line > 153) // lasts 4560 clocks
                        {
                            _line = 0; // reset line
                            _mode = 2; // reenter oam read
                            _vblankTriggered = false;
                        }
                    }
                    break;
            }
        }
    }
}
