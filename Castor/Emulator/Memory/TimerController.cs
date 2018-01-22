using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class TimerController
    {
        private Device _d;

        private int divider;
        private int counter;
        private int modulo;
        private bool enabled = false;
        private int step = 256;
        private int internal_counter;
        private int internal_divider;

        public byte DIV
        {
            get => (byte)divider;
            set => divider = 0;
        }

        public byte TIMA
        {
            get => (byte)counter;
            set => counter = value;
        }

        public byte TMA
        {
            get => (byte)modulo;
            set => modulo = value;
        }

        public byte TAC
        {
            get
            {
                var enable = (byte)(enabled ? 4 : 0);
                var step = (byte)Math.Log(this.step / 16, 2);

                return (byte)(enable | step);
            }

            set
            {
                enabled = (value & 0x4) != 0;
                step = (int)(Math.Pow(value & 0x3, 2) * 16);
            }
        }

        public TimerController(Device d)
        {
            _d = d;
        }

        public void Step(int steps)
        {
            internal_divider += steps;

            while (internal_divider >= 256)
            {
                divider += 1;
                internal_divider -= 256;
            }

            if (enabled)
            {
                internal_counter += steps;

                while (internal_counter >= step)
                {
                    counter += 1;

                    if (counter == 0)
                    {
                        counter = modulo;
                        _d.IRQ.RequestInterrupt(InterruptFlags.Timer);
                    }

                    internal_counter -= step;
                }
            }
        }
    }
}
