using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class InterruptController
    {
        private GameboySystem _system;

        public byte IF;
        public byte IE;

        public InterruptController(GameboySystem system)
        {
            _system = system;
        }
    }
}
