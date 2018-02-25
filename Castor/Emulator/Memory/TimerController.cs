namespace Castor.Emulator.Memory
{
    public class TimerController
    {
        public enum TimerFrequency : byte
        {
            Clocks_1024 = 0,
            Clocks_16 = 1,
            Clocks_64 = 2,
            Clocks_256 = 3
        }

        private Device _d;

        private ushort _internalCounter;

        private TimerFrequency _tac;
        private bool timerEnable;

        private byte _timer;

        public byte DIV
        {
            get
            {
                return (byte)((_internalCounter >> 8) & 0xFF);
            }

            set
            {
                _internalCounter = 0;
            }
        }

        public int TIMA
        {
            get => _timer;
            set
            {
                if (value > byte.MaxValue)
                {
                    _d.IRQ.RequestInterrupt(InterruptFlags.Timer);
                    _timer = TMA;
                }

                else
                {
                    _timer = (byte)value;
                }
            }
        }

        public byte TMA { get; set; }

        public byte TAC
        {
            get
            {
                return (byte)(((timerEnable ? 1 : 0) << 2 | (byte)_tac) | 0xF8);
            }

            set
            {
                timerEnable = ((value >> 2) & 1) == 1;
                _tac = (TimerFrequency)(value & 3);
            }
        }

        public int InternalCounter
        {
            get
            {
                return _internalCounter;
            }


            set
            {
                int temp = _internalCounter;
                _internalCounter = (ushort)value;
                int temp2 = 0;
                if (timerEnable)
                {
                    switch (_tac)
                    {
                        case TimerFrequency.Clocks_1024:
                            temp &= ~1023;
                            temp2 = (_internalCounter - temp) / 1024;
                            TIMA += (byte)temp2;
                            break;
                        case TimerFrequency.Clocks_16:
                            temp &= ~15;
                            temp2 = (_internalCounter - temp) / 16;
                            TIMA += (byte)temp2;
                            break;
                        case TimerFrequency.Clocks_256:
                            temp &= ~255;
                            temp2 = (_internalCounter - temp) / 256;
                            TIMA += (byte)temp2;
                            break;
                        case TimerFrequency.Clocks_64:
                            temp &= ~63;
                            temp2 = (_internalCounter - temp) / 64;
                            TIMA += (byte)temp2;
                            break;
                    }
                }
            }
        }

        public TimerController(Device d)
        {
            _d = d;
            _internalCounter = 0xABCC;
        }

        public void Step(int cycles)
        {
            InternalCounter += cycles;
        }
    }
}
