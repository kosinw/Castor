using System;

namespace Castor.Emulator.Memory
{
    public class InputController
    {
        private Device _d;

        public byte P1
        {
            get
            {
                return 0;
            }

            set
            {
            }
        }

        public InputController(Device d)
        {
            _d = d;
        }
    }
}
