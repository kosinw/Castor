using System;
using System.Text;

namespace Castor.Emulator.Cartridge
{
    public class MBC1 : ICartridge
    {
        private byte[] _bytecode;

        private int _currentRomBank;
        private int _romSize;
        private int _numberOfRomBanks;

        public MBC1(byte[] bytecode)
        {
            _bytecode = bytecode;
            _currentRomBank = 1;

            _romSize = 32 << (bytecode[0x148]);
            _numberOfRomBanks = _romSize / 16;
        }

        public byte this[int idx]
        {
            get
            {
                if (idx < 0x4000)
                    return _bytecode[idx];
                if (idx < 0x8000)
                    return _bytecode[idx + (_currentRomBank * 0x4000)];

                throw new Exception("These addresses are not readable.");
            }

            set
            {
                switch (idx)
                {
                    case var r when r > 0x1FFF && r < 0x4000:
                        {
                            var newRomBank = _currentRomBank;

                            newRomBank = (newRomBank >> 5) << 5;

                            newRomBank |= value;

                            if ((byte)newRomBank > _numberOfRomBanks - 1)
                            {
                                throw new Exception("Invalid ROM Bank chosen.");
                            }

                            _currentRomBank = (byte)newRomBank;
                            break;
                        }

                    case var r when r > 0x3FFF && r < 0x6000:
                        {
                            var newRomBank = _currentRomBank;

                            newRomBank = (_currentRomBank & ~(0x60));

                            newRomBank |= (value << 5);

                            if ((byte)newRomBank > _numberOfRomBanks - 1)
                            {
                                throw new Exception("Invalid ROM Bank chosen.");
                            }

                            _currentRomBank = (byte)newRomBank;
                            break;
                        }
                }
            }
        }

        public string Title
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0x134; i <= 0x142; ++i)
                    builder.Append(Convert.ToChar(this[i]));

                return builder.ToString().Trim();
            }
        }
    }
}
