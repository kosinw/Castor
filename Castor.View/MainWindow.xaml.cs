using Castor.Emulator;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Castor.View
{
    public partial class MainWindow : Window
    {
        GameboySystem _system;
        Thread _systemThread;

        WriteableBitmap _writableBuffer;
        Int32Rect _renderingRect;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize gameboy subsystem
            _system = new GameboySystem();

            // Set DataContext to gameboy system
            // Todo Add interface of INotifyPropertyChanged
            this.DataContext = _system;

            // Initailize systemThread but don't start it
            _systemThread = new Thread(new ThreadStart(_system.Start));

            // Add OnRenderEventHandler
            _system.GPU.OnRenderEvent += GPU_OnRenderEvent;

            // Initialize writableBuffer and bind to image
            _writableBuffer = new WriteableBitmap(160, 144, 96, 96, PixelFormats.Gray8, null);
            canvas.Source = _writableBuffer;

            // This will be used whenever the onrender event is triggered
            _renderingRect = new Int32Rect(0, 0, 160, 144);
        }

        private void GPU_OnRenderEvent(byte[] data)
        {
            Dispatcher.BeginInvoke((Action<byte[]>)Render, data);
        }

        private void Render(byte[] data)
        {
            _writableBuffer.WritePixels(_renderingRect, data, 160, 0);
        }

        private void Menu_LoadROM_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            bool? fileChoosen = fd.ShowDialog();

            if (fileChoosen == true)
            {
                _system.LoadROM(File.ReadAllBytes(fd.FileName));                
            }

            _systemThread.Start();
        }
    }
}
