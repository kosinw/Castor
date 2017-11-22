using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Cartridge
{
    public class MBC0 : ICartridge
    {
        public MBC0(byte[] bytecode)
        {
            this._bytecode = bytecode;
        }

        private byte[] _bytecode;

        public byte this[int idx]
        {
            get
            {
                if (idx < 0x8000)
                    return _bytecode[idx];

                return 0;
            }

            set
            {
                return;
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
        
        public void SetMemoryModel(byte value) => throw new Exception("This cartridge types does not support this feature.");
        public void SwitchROMBank(byte index) => throw new Exception("This cartridge types does not support this feature.");
    }
}
