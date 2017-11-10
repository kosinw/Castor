using Castor.Emulator.Cartridge;
using Castor.Emulator.CPU;
using Castor.Emulator.Memory;
using System;
using System.Drawing;
using Castor.Emulator.Video;

namespace Castor.Emulator
{
    public class GameboySystem
    {
        public Z80 CPU;
        public MemoryMapper MMU;
        public ICartridge Cartridge;
        public VideoController GPU;
        public InterruptController IRC;
        

        public GameboySystem()
        {
            CPU = new Z80(this);
            MMU = new MemoryMapper(this);
            Cartridge = null;
            GPU = new VideoController(this);
            IRC = new InterruptController(this);
        }

        public void LoadROM(byte[] bytecode)
        {
            Cartridge = CartridgeFactory.CreateCartridge(bytecode, this);
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
