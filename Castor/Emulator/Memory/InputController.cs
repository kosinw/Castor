using OpenTK.Input;

namespace Castor.Emulator.Memory
{
    public class InputController
    {
        JoypadSelectState _pressedKeys;
        GamePadState _gamepad;
        KeyboardState _keyboard;
        SelectedKeys _selectedKeys;
        bool usingGamepad;        
        int _gamepadIndex;

        
        public byte P1
        {
            get
            {
                byte header = (byte)((byte)_selectedKeys << 4);
                byte body = 0;

                if (!_selectedKeys.HasFlag(SelectedKeys.Buttons))
                {
                    body |= (byte)(body | (byte)JoypadSelectState.A);
                    body |= (byte)(body | (byte)JoypadSelectState.B);
                    body |= (byte)(body | (byte)JoypadSelectState.Select);
                    body |= (byte)(body | (byte)JoypadSelectState.Start);
                }

                if (!_selectedKeys.HasFlag(SelectedKeys.Direction))
                {
                    body |= (byte)(body | (byte)JoypadSelectState.Down);
                    body |= (byte)(body | (byte)JoypadSelectState.Up);
                    body |= (byte)(body | (byte)JoypadSelectState.Left);
                    body |= (byte)(body | (byte)JoypadSelectState.Right);
                }
                
                return (byte)~(header | body);
            }

            set
            {
                byte header = (byte)(~((value >> 4) & 0b11) & 0b11);

                _selectedKeys |= (SelectedKeys)header;
            }
        }

        public InputController()
        {
            _pressedKeys = JoypadSelectState.None;
            usingGamepad = false;
            _selectedKeys = SelectedKeys.Buttons | SelectedKeys.Direction;
        }

        public void Initialize()
        {
            int gamepadIndex = 0;
            usingGamepad = true;

            do // try to get a connected gamepad
            {
                _gamepad = GamePad.GetState(gamepadIndex);
                gamepadIndex++;
            } while (!_gamepad.IsConnected && gamepadIndex < 4);

            if (!_gamepad.IsConnected) // if a gamepad could not be found
            {
                usingGamepad = false;
                _keyboard = Keyboard.GetState();
            }

            else
            {
                _gamepadIndex = gamepadIndex - 1;
            }
        }

        public void Step()
        {
            if (usingGamepad)
                StepGamepad();
            else
                StepKeyboard();
        }

        private void StepGamepad()
        {
            _gamepad = GamePad.GetState(_gamepadIndex);

            if (_gamepad.Buttons.A == ButtonState.Pressed)
            {
                ;
            }
        }

        private void StepKeyboard()
        {
            _keyboard = Keyboard.GetState();

            if (_keyboard.IsKeyDown(Key.A))
            {
                ;
            }
        }
    }
}
