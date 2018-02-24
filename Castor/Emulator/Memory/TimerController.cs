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

        public byte TIMA
        {
            get => _timer;
            set
            {
                if (value <= byte.MaxValue)                
                {
                    _timer = value;
                }
                
                else
                {
                    _d.IRQ.RequestInterrupt(InterruptFlags.Timer);
                    _timer = TMA;
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
                _internalCounter = (ushort)value;

                switch (_tac)
                {
                    case TimerFrequency.Clocks_1024:
                        if (_internalCounter % 1024 == 0)
                        {
                            TIMA += 1;
                        }
                        break;
                    case TimerFrequency.Clocks_16:
                        if (_internalCounter % 16 == 0)
                        {
                            TIMA += 1;
                        }
                        break;
                    case TimerFrequency.Clocks_256:
                        if (_internalCounter % 256 == 0)
                        {
                            TIMA += 1;
                        }
                        break;
                    case TimerFrequency.Clocks_64:
                        if (_internalCounter % 64 == 0)
                        {
                            TIMA += 1;
                        }
                        break;
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
