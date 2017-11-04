using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class BootROM : IAddressableComponent
    {
        private byte[] _bytecode;

        public BootROM()
        {
            // TODO: Define this file in the assembly info
            _bytecode = File.ReadAllBytes(@"C:\Users\Owner\Documents\GameboyEmulator\GameboyEmulator\ROMs\DMG_ROM.bin");
        }

        public int Count() => _bytecode.Length;

        public byte this[int idx]
        {
            get => _bytecode[idx];
            set => _bytecode[idx] = value;
        }
    }
}
