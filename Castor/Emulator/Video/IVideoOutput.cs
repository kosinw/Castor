using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Video
{
    public interface IVideoOutput
    {
        void DrawFrame(ColorPallette[,] framebuffer);
    }
}
