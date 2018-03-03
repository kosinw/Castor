using Castor.Emulator;
using Castor.Emulator.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Castor.GL
{
    public class CastorGame : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spritebatch;

        Texture2D _backbuffer;
        Device _emulator = new Device();

#if (GREENSCALE)
        Color _tintColor = new Color(0xE1, 0xF8, 0xD1);
#else
        Color _tintColor = Color.White;
#endif


        public CastorGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

            Window.AllowUserResizing = true;
            Window.Title = "Castor";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            TargetElapsedTime = System.TimeSpan.FromMilliseconds(1000.0f / 59.7f);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
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
            }
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState gps = GamePad.GetState(PlayerIndex.One);
            KeyboardState kps = Keyboard.GetState();

            _emulator.Frame();

            _emulator.JOYP[InputController.Index.A] = gps.IsButtonDown(Buttons.A) || kps.IsKeyDown(Keys.Z);
            _emulator.JOYP[InputController.Index.B] = gps.IsButtonDown(Buttons.B) || kps.IsKeyDown(Keys.X);
            _emulator.JOYP[InputController.Index.UP] = gps.IsButtonDown(Buttons.DPadUp) || kps.IsKeyDown(Keys.Up);
            _emulator.JOYP[InputController.Index.DOWN] = gps.IsButtonDown(Buttons.DPadDown) || kps.IsKeyDown(Keys.Down);
            _emulator.JOYP[InputController.Index.LEFT] = gps.IsButtonDown(Buttons.DPadLeft) || kps.IsKeyDown(Keys.Left);
            _emulator.JOYP[InputController.Index.RIGHT] = gps.IsButtonDown(Buttons.DPadRight) || kps.IsKeyDown(Keys.Right);
            _emulator.JOYP[InputController.Index.START] = gps.IsButtonDown(Buttons.Start) || kps.IsKeyDown(Keys.Enter);
            _emulator.JOYP[InputController.Index.SELECT] = gps.IsButtonDown(Buttons.Back) || kps.IsKeyDown(Keys.Back);

            if (kps.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (_backbuffer != null)
            {
                byte[] backbuffer = _emulator.GPU.GetScreenBuffer();
                _backbuffer.SetData(backbuffer);
            }

            base.Update(gameTime);
        }

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

            _spritebatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Texture);
            _spritebatch.Draw(_backbuffer, bounds, _tintColor);
            _spritebatch.End();

            base.Draw(gameTime);
        }
    }
}
