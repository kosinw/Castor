using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class MemoryMapper : IAddressableComponent
    {
        private GameboySystem _system;
        private byte[] _internalRam;
        private byte[] _internalRam2;
        private BootROM _bootROM = new BootROM();

        private bool _mapFirst256BootROM = true;

        public MemoryMapper(GameboySystem system)
        {
            _system = system;
            _internalRam = new byte[0x2000];
            _internalRam2 = new byte[0x80];
        }

        public byte this[int idx]
        {
            get
            {
                if (idx < 0x0100 && _mapFirst256BootROM)
                    return _bootROM[idx];
                else if (idx < 0x8000)
                    return _system.Cartridge[idx];
                else if (idx < 0xA000)
                    return _system.GPU[idx];
                else if (idx < 0xC000)
                    return _system.Cartridge[idx];
                else if (idx < 0xE000)
                    return _internalRam[idx - 0xC000];
                else if (idx < 0xFE00)
                    return _internalRam[idx - 0xE000];
                else if (idx < 0xFEA0)
                    return _system.GPU[idx];
                else if (idx < 0xFF00)
                    return 0;
                else if (idx < 0xFF4C) // here we get a little more complex
                {
                    switch (idx)
                    {
                        case 0xFF0F:
                            return _system.CPU.IF;
                        default:
                            return 0;
                    }
                }
                else if (idx < 0xFF80)
                    return 0;
                else if (idx < 0xFFFF)
                    return _internalRam2[idx - 0xFF80];
                else if (idx == 0xFFFF)
                    return _system.CPU.IE;

                throw new Exception("You may not read to this memory location.");
            }
            
            set
            {
                if (idx < 0x8000)
                    _system.Cartridge[idx] = value;
                else if (idx < 0xA000)
                    _system.GPU[idx] = value;
                else if (idx < 0xC000)
                    _system.Cartridge[idx] = value;
                else if (idx < 0xE000)
                    _internalRam[idx - 0xC000] = value;
                else if (idx < 0xFE00)
                    _internalRam[idx - 0xE000] = value;
                else if (idx < 0xFEA0)
                    _system.GPU[idx] = value;
                else if (idx < 0xFF00)
                    return;
                else if (idx < 0xFF4C)
                {
                }
                else if (idx == 0xFF50)
                    _mapFirst256BootROM = false;
                else if (idx < 0xFF80)
                    return;
                else if (idx < 0xFFFF)
                    _internalRam2[idx - 0xFF80] = value;
                else if (idx == 0xFFFF)
                    _system.CPU.IE = value;                
            }
        }
    }
}
