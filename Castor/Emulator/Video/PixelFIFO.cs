using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public class PixelFIFO
    {
        private PixelMetadata[] _pixelFifo;

        private int _elements;

        public bool Enabled { get => Count > 8; }
        public int Count { get => _pixelFifo.Length; }

        public PixelFIFO()
        {
            _pixelFifo = new PixelMetadata[16];
            _elements = 0;
        }

        /// <summary>
        /// This will be cleared only when it has been switched to window mode.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_pixelFifo, 0, _pixelFifo.Length);
            _elements = 0;
        }

        /// <summary>
        /// This is used to enqueue pixels onto the pixel fifo.
        /// </summary>
        /// <param name="metadata"></param>
        public void Enqueue(PixelMetadata metadata)
        {            
            _pixelFifo[_elements++] = metadata;
        }


        /// <summary>
        /// This is used to dequeue pixels off of the pixel fifo.
        /// </summary>
        /// <returns></returns>
        public PixelMetadata Dequeue()
        {
            if (_elements == 0)
                throw new IndexOutOfRangeException("FIFO only has 0 elements.");

            PixelMetadata metadata = _pixelFifo.ElementAt(--_elements);            
            return metadata;
        }
    }
}
