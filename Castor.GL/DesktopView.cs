using Castor.Emulator;
using Castor.Emulator.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Castor.GL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class DesktopView : Game
    {
        [DllImport("winmm.dll")]
        internal static extern uint timeBeginPeriod(uint period);
        [DllImport("winmm.dll")]
        internal static extern uint timeEndPeriod(uint period);

        GraphicsDeviceManager _graphics;
        SpriteBatch _spritebatch;

        Thread _emulatorthread;
        Texture2D _backbuffer;
        Device _emulator = new Device();


        public DesktopView()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            Window.AllowUserResizing = true;
            Window.Title = "Castor";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spritebatch = new SpriteBatch(GraphicsDevice);

            _backbuffer = new Texture2D(GraphicsDevice, 160, 144);

            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog
            {
                DefaultExt = ".gb",
                Filter = "Gameboy ROM Files (.gb)|*.gb",
                Multiselect = false
            };


            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var filename = fileDialog.FileName;

                byte[] bytecode = File.ReadAllBytes(filename);
                _emulator.LoadROM(bytecode);

                _emulatorthread = new Thread(new ThreadStart(EmulatorCoroutine));
                _emulatorthread.Start();
            }

            else
            {
                if (_emulatorthread != null)
                    _emulatorthread.Abort();

                System.Environment.Exit(0);
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (_emulatorthread != null)
                _emulatorthread.Abort();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState gps = GamePad.GetState(PlayerIndex.One);

            _emulator.JOYP[InputController.Index.A] = gps.IsButtonDown(Buttons.A);
            _emulator.JOYP[InputController.Index.B] = gps.IsButtonDown(Buttons.B);
            _emulator.JOYP[InputController.Index.UP] = gps.IsButtonDown(Buttons.DPadUp);
            _emulator.JOYP[InputController.Index.DOWN] = gps.IsButtonDown(Buttons.DPadDown);
            _emulator.JOYP[InputController.Index.LEFT] = gps.IsButtonDown(Buttons.DPadLeft);
            _emulator.JOYP[InputController.Index.RIGHT] = gps.IsButtonDown(Buttons.DPadRight);
            _emulator.JOYP[InputController.Index.START] = gps.IsButtonDown(Buttons.Start);
            _emulator.JOYP[InputController.Index.SELECT] = gps.IsButtonDown(Buttons.Back);

            if (_backbuffer != null)
            {
                byte[] backbuffer = _emulator.GPU.GetScreenBuffer();
                _backbuffer.SetData(backbuffer);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            Rectangle bounds = GraphicsDevice.Viewport.Bounds;

            float asp = GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
            float targetAsp = 160.0f / 144.0f;

            if (asp > targetAsp)
            {
                int targetWidth = (int)(bounds.Height * targetAsp);
                bounds.X = (bounds.Width - targetWidth) / 2;
                bounds.Width = targetWidth;
            }
            else if (asp < targetAsp)
            {
                int targetHeight = (int)(bounds.Width / targetAsp);
                bounds.Y = (bounds.Height - targetHeight) / 2;
                bounds.Height = targetHeight;
            }

            _spritebatch.Begin(samplerState: SamplerState.PointClamp);
            _spritebatch.Draw(_backbuffer, bounds, Color.White);
            _spritebatch.End();

            base.Draw(gameTime);
        }

        private void EmulatorCoroutine()
        {
            Stopwatch watch = Stopwatch.StartNew();
            System.TimeSpan dt = System.TimeSpan.FromSeconds(1.0 / 60.0);
            System.TimeSpan elapsedTime = System.TimeSpan.Zero;

            while (true)
            {
                watch.Restart();

                _emulator.Frame();

                elapsedTime = watch.Elapsed;
                if (elapsedTime < dt)
                {
                    timeBeginPeriod(1); // this is to increase the resolution of Windows' system clock
                    Thread.Sleep(dt - elapsedTime);
                    timeEndPeriod(1);
                }
            }
        }
    }
}
