using Castor.Emulator.Cartridge;
using Castor.Emulator.CPU;
using Castor.Emulator.Memory;
using Castor.Emulator.Video;

namespace Castor.Emulator
{
    public class Device
    {
        const int MAX_CYCLES = 4_194_304;

        public Z80 CPU;
        public MemoryMapper MMU;
        public ICartridge Cartridge;
        public VideoController GPU;
        public InterruptController IRQ;
        public DMAController DMA;
        public InputController JOYP;
        public TimerController TIM;

        public Device()
        {
            CPU = new Z80(this);
            DMA = new DMAController(this);
            MMU = new MemoryMapper(this);
            Cartridge = null;
            GPU = new VideoController(this);
            IRQ = new InterruptController();
            TIM = new TimerController(this);
            JOYP = new InputController(this);
        }

        public void LoadROM(byte[] bytecode)
        {
            Cartridge = CartridgeFactory.CreateCartridge(bytecode);
        }

        /// <summary>
        /// This method tells the gameboy system to emulate one frame.
        /// (70,224 clock cycles ~= One emulator video frame.)
        /// Poll a gamepad/keyboard input after every vsync.
        /// </summary>
        public void Frame()
        {
            for (int _counter = 0; _counter < MAX_CYCLES / 60;)
            {
                int cycles = CPU.Step();
                GPU.Step(cycles);
                DMA.Step(cycles);
                TIM.Step(cycles);

                _counter += cycles; // need to add the cycles used up
            }            
        }
    }
}
