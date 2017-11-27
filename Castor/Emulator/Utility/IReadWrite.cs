using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    public interface IReadWrite<T>
    {
        Func<T> Read { get; }
        Action<T> Write { get; }
    }
}
