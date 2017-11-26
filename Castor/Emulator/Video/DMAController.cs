using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public class DMAController
    {
        GameboySystem _system;

        private int _dmaclock;
        private bool _dmaenabled;
        private int _bytescopied;
        private ushort _copyzero;

        const int SETUP_CLOCKS = 4;
        const ushort OAM_ZERO = 0xFE00;

        public bool IsTransfering
        {
            get => _dmaenabled;
        }

        public DMAController(GameboySystem system)
        {
            _system = system;
        }
        
        public void BeginDMA(byte startAddress)
        {
            _dmaclock = 0;
            _bytescopied = 0;
            _dmaenabled = true;
            _copyzero = (ushort)(startAddress * 0x100);
        }

        /// <summary>
        /// One OAM DMA Transfer takes 644 CPU clocks.
        /// This copies memory from ROM to OAM.
        /// A total amount of 160 bytes are copied.
        /// </summary>
        /// <param name="cycles"></param>
        public void Step(int cycles)
        {
            if (!_dmaenabled)
                return;

            _dmaclock += cycles;

            // while their enough clocks have passed by to copy another byte
            // one byte takes 4 clocks
            // there is a setup delay of 4 clocks
            while (_dmaclock - SETUP_CLOCKS >= _bytescopied * 4)
            {
                int _ = 0; // don't really carry about cycles

                byte from = _system.MMU.ReadByte(_copyzero + _bytescopied - 1, ref _);
                _system.MMU.WriteByte(OAM_ZERO + _bytescopied - 1, from, ref _);

                _bytescopied++;
            }

            if (_dmaclock >= 644)
            {
                EndDMA();
            }
        }

        public void EndDMA()
        {
            _dmaclock = 0;
            _bytescopied = 0;
            _dmaenabled = false;
        }
    }
}
