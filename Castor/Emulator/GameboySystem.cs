﻿using Castor.Emulator.Cartridge;
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
        public InterruptController ISR;
        public DMAController DMA;

        public GameboySystem()
        {                        
            CPU = new Z80(this);
            MMU = new MemoryMapper(this);
            Cartridge = null;
            GPU = new VideoController(this);
            ISR = new InterruptController(this);
            DMA = new DMAController(this);
        }

        public void LoadROM(byte[] bytecode)
        {
            Cartridge = CartridgeFactory.CreateCartridge(bytecode);
        }

        /// <summary>
        /// This method tells the gameboy system to emulate one frame.
        /// (70,224 clock cycles ~= One emulator video frame.)
        /// </summary>
        public void Frame()
        {
            for (int _counter = 0; _counter < 70_224;)
            {
                int cycles = CPU.Step();
                GPU.Step(cycles);

                if (DMA.IsTransfering)
                    DMA.Step(cycles);

                _counter += cycles; // need to add the cycles used up
            }
        }
    }
}
