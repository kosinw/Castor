using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Cartridge
{
    public class ROMOnlyCartridge : ICartridge
    {
        public ROMOnlyCartridge(byte[] _bytecode)
        {
            this._bytecode = _bytecode;
        }

        private byte[] _bytecode;

        public byte this[int idx]
        {
            get
            {
                if (idx < 0x8000)
                    return _bytecode[idx];

                throw new Exception("You may not read to this location in memory.");
            }

            set => throw new Exception("You may not write to this location in memory.");
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
        
        public void SetMemoryModel(byte value) => throw new Exception("This cartridge types does not support this feature.");
        public void SwitchROMBank(byte index) => throw new Exception("This cartridge types does not support this feature.");
    }
}
