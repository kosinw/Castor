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

        public TimerController(Device d)
        {
            _d = d;
        }        
    }
}
