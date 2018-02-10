using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Cartridge
{
    public interface ICartridge
    {
        string Title { get; }
        byte this[int idx] { get; set; }
    }
}
