using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public class DMAController
    {
        private bool _enabled;
        private int _zeroAddress;
        private int _cycles;
        private int _bytescopied;

        private Device _system;

        public DMAController(Device system)
        {
            _system = system;
            _enabled = false;
            _zeroAddress = 0;
            _cycles = 0;
            _bytescopied = 0;
        }

        public void BeginOAMTransfer(ushort addr)
        {
            _enabled = true;
            _zeroAddress = addr * 0x100;
            _cycles = 0;
            _bytescopied = 0;
        }

        public void Step(int cycles)
        {
            if (!_enabled)
                return;

            _cycles += cycles;

            while ((_cycles / 4) - 1 >= _bytescopied)
            {
                _system.MMU[_zeroAddress + _bytescopied] = _system.MMU[_zeroAddress + _bytescopied];
                _bytescopied++;
            }

            if (_cycles >= 644)
            {
                _enabled = false;
            }
        }
    }
}
