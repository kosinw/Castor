using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public class PixelFIFO
    {
        private LinkedList<PixelMetadata> _pixelFifo;
        public bool Enabled { get => Count > 8; }

        public int Count { get => _pixelFifo.Count; }

        /// <summary>
        /// This will be cleared only when it has been switched to window mode.
        /// </summary>
        public void Clear() => _pixelFifo.Clear();

        /// <summary>
        /// This is used to enqueue pixels onto the pixel fifo.
        /// </summary>
        /// <param name="metadata"></param>
        public void Enqueue(PixelMetadata metadata)
        {
            _pixelFifo.AddLast(metadata);
        }


        /// <summary>
        /// This is used to dequeue pixels off of the pixel fifo.
        /// </summary>
        /// <returns></returns>
        public PixelMetadata Dequeue()
        {
            PixelMetadata metadata = _pixelFifo.ElementAt(0);
            _pixelFifo.RemoveFirst();
            return metadata;
        }
    }
}
