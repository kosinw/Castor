using Castor.Emulator;
using Castor.Emulator.Video;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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

        [DllImport("winmm.dll")]
        internal static extern uint timeBeginPeriod(uint period);
        [DllImport("winmm.dll")]
        internal static extern uint timeEndPeriod(uint period);

        public MainWindow()
        {
            InitializeComponent();

            // Initialize gameboy subsystem
            _system = new GameboySystem();

            this.DataContext = _system;

            // Add OnRenderEventHandler
            _system.GPU.OnRenderEvent += GPU_OnRenderEvent;

            // Initialize writableBuffer and bind to image
            _writableBuffer = new WriteableBitmap(VideoController.RENDER_WIDTH,
                VideoController.RENDER_HEIGHT, 96, 96, PixelFormats.Gray8, null);

            canvas.Source = _writableBuffer;

            // This will be used whenever the onrender event is triggered
            _renderingRect = new Int32Rect(0, 0, VideoController.RENDER_WIDTH,
                VideoController.RENDER_HEIGHT);
        }

        private void GPU_OnRenderEvent()
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                _writableBuffer.WritePixels(_renderingRect, _system.GPU.Screen, 
                    VideoController.RENDER_WIDTH * VideoController.RENDER_HEIGHT, _writableBuffer.BackBufferStride);
            });
        }

        private void Menu_LoadROM_Click(object sender, RoutedEventArgs e)
        {

            if (_systemThread != null)
            {
                _systemThread.Join(1000);

                _system = new GameboySystem();
                _system.GPU.OnRenderEvent += GPU_OnRenderEvent;
            }

            OpenFileDialog fd = new OpenFileDialog();

            if (fd?.ShowDialog() == true)
            {
                _system.LoadROM(File.ReadAllBytes(fd.FileName));

                _systemThread = new Thread(new ThreadStart(delegate ()
                {
                    Stopwatch watch = new Stopwatch();
                    while (true)
                    {
                        watch.Reset();
                        watch.Start();
                        _system.Frame();
                        watch.Stop();

                        if (watch.ElapsedMilliseconds < 16)
                        {
                            timeBeginPeriod(1); // this is to increase the resolution of window's clock
                            Thread.Sleep(16 - (int)watch.ElapsedMilliseconds);
                            timeEndPeriod(1);
                        }
                    }
                }))
                {
                    Priority = ThreadPriority.Highest
                };

                _systemThread.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_systemThread != null)
            {
                _systemThread.Abort();
            }
        }
    }
}
