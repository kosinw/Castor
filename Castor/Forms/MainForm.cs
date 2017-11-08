using Castor.Emulator;
using Castor.Emulator.Video;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Castor.Forms
{
    public partial class MainForm : Form, IVideoOutput
    {
        public MainForm()
        {
            InitializeComponent();
        }

        GameboySystem _system;

        private void OnOpenFileDialog(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            if (fd.ShowDialog() == DialogResult.OK)
            {
                byte[] bytecode = File.ReadAllBytes(fd.FileName);
                _system = new GameboySystem(bytecode, this);
                _system.Start();
            }
        }

        public unsafe void DrawFrame(ColorPallette[,] framebuffer)
        {
            BitmapData bmpData = new BitmapData();            
        }
    }
}
