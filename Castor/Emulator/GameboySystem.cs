using Castor.Emulator.Cartridge;
using Castor.Emulator.CPU;
using Castor.Emulator.Memory;
using System;
using System.Drawing;
using Castor.Emulator.Video;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Castor.Emulator
{
    public class GameboySystem : INotifyPropertyChanged
    {
        public Z80 CPU;
        public MemoryMapper MMU;
        public ICartridge Cartridge;
        public VideoController GPU;
        public InterruptController IRC;

        public event PropertyChangedEventHandler PropertyChanged;

        public GameboySystem()
        {
            CPU = new Z80(this);
            MMU = new MemoryMapper(this);
            Cartridge = null;
            GPU = new VideoController(this);
            IRC = new InterruptController(this);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void LoadROM(byte[] bytecode)
        {
            Cartridge = CartridgeFactory.CreateCartridge(bytecode);
        }

        public void Start()
        {
            // TODO: Add bootup state + boot rom for Gameboy here
            Restart();
        }

        public void Restart()
        {
            CPU.PC = 0;
        }

        public void Frame()
        {
            for (int _counter = 0; _counter < 70_224; _counter++)
            {
                CPU.Step();
                GPU.Step();
            }            
        }
    }
}
