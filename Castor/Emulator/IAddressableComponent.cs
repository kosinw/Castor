using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator
{
    public interface IAddressableComponent
    {
        /// <summary>
        /// This is a memory mappable component indexer. 
        /// </summary>
        /// <returns></returns>
        byte this[int idx] { get; set; }
    }
}
