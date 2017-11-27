using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Utility
{
    /// <summary>
    /// This is a memory refence to any integral type (mostly ushorts, bytes, and sbytes);
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemoryReference<T> : IReadWrite<T>
        where T : IComparable<T>, IFormattable, IConvertible, IEquatable<T>
    {
        public Func<T> Read { get; set; }
        public Action<T> Write { get; set; }
    }
}
