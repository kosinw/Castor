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

        private Timer _timer;

        public GameboySystem(byte[] bytecode)
        {
            CPU = new Z80(this);
            MMU = new MemoryMapper(this);
            Cartridge = CartridgeFactory.CreateCartridge(bytecode);
            GPU = new VideoController(this);

            _timer = new Timer();
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

        public void OnClockCycle(object sender, EventArgs e) // just to 1171 ticks per event
        {
            _timer.Stop(); // make sure the timer doesn't trigger another event

            for (int i = 0; i < 66_666; ++i) // cycles = clock speed in Hz / required fps
                CPU.Step();
            
            _timer.Start(); // restart the timer in order to start triggering events agains
        }
    }
}
