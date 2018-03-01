using System;

namespace Castor.GL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new DesktopView())
            {
                game.Run();
            }
        }
    }
}
