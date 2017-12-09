using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class BootROM
    {
        private byte[] _bytecode;

        public BootROM()
        {
            _bytecode = Castor.Properties.Resources.DMG_ROM;
        }

        public int Count() => _bytecode.Length;

        public byte this[int idx]
        {
            get => _bytecode[idx];
            set => _bytecode[idx] = value;
        }
    }
}
