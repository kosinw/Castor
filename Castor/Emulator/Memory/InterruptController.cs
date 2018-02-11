using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class InterruptController
    {
        public byte IF { get => (byte)_if; set => _if = (InterruptFlags)value; }
        public byte IE { get => (byte)_ie; set => _ie = (InterruptFlags)value; }

        private InterruptFlags _if;
        private InterruptFlags _ie;

        /// <summary>
        /// This method is meant to test if the corresponding bits of an interrupt are set on IF and IE.
        /// </summary>
        /// <param name="flag">The interrupt flag, (only one is allowed).</param>
        /// <returns></returns>
        public bool CanHandleInterrupt(InterruptFlags flag)
        {
            return (_if.HasFlag(flag) && _ie.HasFlag(flag));
        }

        /// <summary>
        /// Set flag bit on interrupt register.
        /// </summary>
        /// <param name="flag"></param>
        public void RequestInterrupt(InterruptFlags flag)
        {
            _if |= flag;
        }

        public void DisableInterrupt(InterruptFlags flag)
        {
            _if &= ~flag;
        }

        public bool CanServiceInterrupts
        {
            get => (_ie & _if) != 0;
        }
    }
}
