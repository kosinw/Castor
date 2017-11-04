using Castor.Emulator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Castor.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        GameboySystem system;

        private void OnOpenFileDialog(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            if (fd.ShowDialog() == DialogResult.OK)
            {
                byte[] bytecode = File.ReadAllBytes(fd.FileName);
                system = new GameboySystem(bytecode);
                system.Start();
            }
        }
    }
}
