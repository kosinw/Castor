using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class InputController
    {
        private bool[] _keys = new bool[]
        {
            false, // Start
            false, // Select
            false, // B
            false, // A
            false, // Down
            false, // Up
            false, // Left
            false, // Right
        };

        private bool _buttonSelect = false;
        private bool _directionSelect = false;
        private Device _d;

        public InputController(Device d)
        {
            _d = d;
        }

        public byte P1
        {
            get
            {
                var b5 = Convert.ToUInt32(_buttonSelect) << 5;

                var b4 = Convert.ToUInt32(_directionSelect) << 4;

                var b3 = (Convert.ToUInt32(_keys[Index.DOWN] && _directionSelect) |
                    Convert.ToUInt32(_keys[Index.START] && _buttonSelect)) << 3;

                var b2 = (Convert.ToUInt32(_keys[Index.UP] && _directionSelect) |
                    Convert.ToUInt32(_keys[Index.SELECT] && _buttonSelect)) << 2;

                var b1 = (Convert.ToUInt32(_keys[Index.LEFT] && _directionSelect) |
                    Convert.ToUInt32(_keys[Index.B] && _buttonSelect)) << 1;

                var b0 = (Convert.ToUInt32(_keys[Index.RIGHT] && _directionSelect) |
                    Convert.ToUInt32(_keys[Index.A] && _buttonSelect)) << 0;

                return (byte)~(b5 | b4 | b3 | b2 | b1 | b0);
            }

            set
            {
                var b5 = !Convert.ToBoolean((value >> 5) & 1);
                var b4 = !Convert.ToBoolean((value >> 4) & 1);

                _buttonSelect = b5;
                _directionSelect = b4;
            }
        }

        public static class Index
        {
            public const int START = 0;
            public const int SELECT = 1;
            public const int B = 2;
            public const int A = 3;
            public const int DOWN = 4;
            public const int UP = 5;
            public const int LEFT = 6;
            public const int RIGHT = 7;
        }

        public bool this[int idx]
        {
            set
            {
                if (_keys[idx] != value)
                {
                    _d.IRQ.RequestInterrupt(InterruptFlags.Joypad);
                }

                _keys[idx] = value;
            }
        }
    }
}
