using Castor.Emulator.Cartridge;
using Castor.Emulator.CPU;
using Castor.Emulator.Memory;
using System;
using System.Drawing;
using Timer = System.Timers.Timer;
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

        private Timer _timer;

        public GameboySystem()
        {
            CPU = new Z80(this);
            MMU = new MemoryMapper(this);
            Cartridge = null;
            GPU = new VideoController(this);
            IRC = new InterruptController(this);

            _timer = new Timer();
        }

        public void LoadROM(byte[] bytecode)
        {
            Cartridge = CartridgeFactory.CreateCartridge(bytecode);
        }

        public void Start()
        {
            // TODO: Add bootup state + boot rom for Gameboy here
            Restart();

            // Set to 60fps in milliseconds
            _timer.Interval = 1000 / 60;
            _timer.Elapsed += OnClockCycle;
            _timer.Enabled = true;
        }

        public void Restart()
        {
            CPU.PC = 0;            
        }

        public void OnClockCycle(object sender, EventArgs e)
        {
            _timer.Stop();

            // According to Pan Docs, it takes 70224 cycles to complete screen refresh
            for (int i = 0; i < 70_224; ++i)
            {
                CPU.Step();
                GPU.Step();
            }

            _timer.Start();
        }
    }
}
