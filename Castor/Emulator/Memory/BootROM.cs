using System.IO;

namespace Castor.Emulator.Memory
{
    public class BootROM
    {
        private byte[] _bytecode;

        public BootROM()
        {
            _bytecode = Properties.Resources.BIOS;
        }

        public int Count() => _bytecode.Length;

        public byte this[int idx]
        {
            get => _bytecode[idx];
            set => _bytecode[idx] = value;
        }
    }
}
