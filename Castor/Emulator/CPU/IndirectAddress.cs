using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public enum IndirectAddress
    {
        AddrHL,
        ZeroPage,
        ZeroPageC,
        AddrHLI,
        AddrHLD
    }
}
