using System;
using System.Collections.Generic;

namespace Castor.Emulator.Memory
{
    public class MemoryMapper
    {
        private Device _d;
        private byte[] _wram;
        private byte[] _zram;
        private BootROM _bootROM = new BootROM();

        public bool _enableBIOS = true;

        private List<byte> _sbString = new List<byte>();

        private byte _sb;
        private byte _sc;

        private byte SB
        {
            get => _sb;
            set
            {
                _sb = value;
                if (_sc == 0x81)
                {
                    _sbString.Add(value);
                }
            }
        }

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
                    return 0;
                else if (idx < 0xFF4C) // here we get a little more complex
                {
                    switch (idx)
                    {
                        case 0xFF00:
                            return _d.JOYP.P1;
                        case 0xFF01:
                            return SB;
                        case 0xFF02:
                            return _sc;
                        case 0xFF04:
                            return _d.TIM.DIV;
                        case 0xFF05:
                            return (byte)_d.TIM.TIMA;
                        case 0xFF06:
                            return _d.TIM.TMA;
                        case 0xFF07:
                            return _d.TIM.TAC;
                        case 0xFF0F:
                            return _d.IRQ.IF;
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
                            throw new Exception("You may not read to this memory location.");
                    }
                }
                else if (idx < 0xFF80)
                    return 0;
                else if (idx < 0xFFFF)
                    return _zram[idx - 0xFF80];
                else if (idx == 0xFFFF)
                    return _d.IRQ.IE;

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
                        case 0xFF00:
                            _d.JOYP.P1 = value;
                            break;
                        case 0xFF01:
                            SB = value;
                            break;
                        case 0xFF02:
                            _sc = value;
                            break;
                        case 0xFF04:
                            _d.TIM.DIV = value;
                            break;
                        case 0xFF05:
                            _d.TIM.TIMA = value;
                            break;
                        case 0xFF06:
                            _d.TIM.TMA = value;
                            break;
                        case 0xFF07:
                            _d.TIM.TAC = value;
                            break;
                        case 0xFF0F:
                            _d.IRQ.IF = value;
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
                    _d.IRQ.IE = value;
            }
        }
    }
}
