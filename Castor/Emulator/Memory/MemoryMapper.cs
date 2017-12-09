using Castor.Emulator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class MemoryMapper
    {
        private Device _d;
        private byte[] _wram;
        private byte[] _zram;
        private BootROM _bootROM = new BootROM();

        private bool _enableBIOS = true;

        public MemoryMapper(Device system)
        {
            _d = system;
            _wram = new byte[0x2000];
            _zram = new byte[0x80];
        }                

        public byte this[int idx]
        {
            get
            {
                if (idx < 0x0100 && _enableBIOS)
                    return _bootROM[idx];
                else if (idx < 0x8000)
                    return _d.Cartridge[idx];
                else if (idx < 0xA000)
                    return _d.GPU[idx];
                else if (idx < 0xC000)
                    return _d.Cartridge[idx];
                else if (idx < 0xE000)
                    return _wram[idx - 0xC000];
                else if (idx < 0xFE00)
                    return _wram[idx - 0xE000];
                else if (idx < 0xFEA0)
                    return _d.GPU[idx];
                else if (idx < 0xFF00)
                    return Consts.NullRef;
                else if (idx < 0xFF4C) // here we get a little more complex
                {
                    switch (idx)
                    {
                        case 0xFF0F:
                            return _d.ISR.IF;
                        case 0xFF40:
                            return _d.GPU.LCDC;
                        case 0xFF41:
                            return _d.GPU.STAT;
                        case 0xFF42:
                            return _d.GPU.SCY;
                        case 0xFF43:
                            return _d.GPU.SCX;
                        case 0xFF44:
                            return _d.GPU.LY;
                        case 0xFF45:
                            return _d.GPU.LYC;
                        case 0xFF47:
                            return _d.GPU.BGP;
                        case 0xFF48:
                            return _d.GPU.OBP0;
                        case 0xFF49:
                            return _d.GPU.OBP1;
                        default:
                            return 0;
                    }
                }
                else if (idx < 0xFF80)
                    return 0;
                else if (idx < 0xFFFF)
                    return _zram[idx - 0xFF80];
                else if (idx == 0xFFFF)
                    return _d.ISR.IE;

                throw new Exception("You may not read to this memory location.");
            }
            
            set
            {
                if (idx < 0x8000)
                    _d.Cartridge[idx] = value;
                else if (idx < 0xA000)
                    _d.GPU[idx] = value;
                else if (idx < 0xC000)
                    _d.Cartridge[idx] = value;
                else if (idx < 0xE000)
                    _wram[idx - 0xC000] = value;
                else if (idx < 0xFE00)
                    _wram[idx - 0xE000] = value;
                else if (idx < 0xFEA0)
                    _d.GPU[idx] = value;
                else if (idx < 0xFF00)
                    return;
                else if (idx < 0xFF4C)
                {
                    switch (idx)
                    {
                        case 0xFF0F:
                            _d.ISR.IF = value;
                            break;
                        case 0xFF40:
                            _d.GPU.LCDC = value;
                            break;
                        case 0xFF41:
                            _d.GPU.STAT = value;
                            break;
                        case 0xFF42:
                            _d.GPU.SCY = value;
                            break;
                        case 0xFF43:
                            _d.GPU.SCX = value;
                            break;
                        case 0xFF44:
                            // this is supposed to write to LY, but ignore it
                            break;
                        case 0xFF45:
                            _d.GPU.LYC = value;
                            break;
                        case 0xFF46:
                            _d.DMA.BeginOAMTransfer(value);
                            break;
                        case 0xFF47:
                            _d.GPU.BGP = value;
                            break;
                        case 0xFF48:
                            _d.GPU.OBP0 = value;
                            break;
                        case 0xFF49:
                            _d.GPU.OBP1 = value;
                            break;
                        default:
                            return;
                    }
                }
                else if (idx == 0xFF50)
                    _enableBIOS = false;
                else if (idx < 0xFF80)
                    return;
                else if (idx < 0xFFFF)
                    _zram[idx - 0xFF80] = value;
                else if (idx == 0xFFFF)
                    _d.ISR.IE = value;                
            }
        }
    }
}
