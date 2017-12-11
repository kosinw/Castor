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

                if (_selectedKeys.HasFlag(SelectedKeys.Buttons))
                {
                    body |= (byte)(_pressedKeys | JoypadSelectState.A);
                    body |= (byte)(_pressedKeys | JoypadSelectState.B);
                    body |= (byte)(_pressedKeys | JoypadSelectState.Select);
                    body |= (byte)(_pressedKeys | JoypadSelectState.Start);
                }

                if (_selectedKeys.HasFlag(SelectedKeys.Direction))
                {
                    body |= (byte)(_pressedKeys | JoypadSelectState.Down);
                    body |= (byte)(_pressedKeys | JoypadSelectState.Up);
                    body |= (byte)(_pressedKeys | JoypadSelectState.Left);
                    body |= (byte)(_pressedKeys | JoypadSelectState.Right);
                }

                return (byte)~(header | body);
            }

            set
            {
                byte header = (byte)(~(value >> 4) & 0b11);

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

            if (!_gamepad.IsConnected)
            {
                usingGamepad = false;
                StepKeyboard();
            }
        }

        private void StepKeyboard()
        {
            _keyboard = Keyboard.GetState();

            if (_keyboard.IsAnyKeyDown)
            {
                if (_keyboard.IsKeyDown(Key.Up))
                    _pressedKeys |= JoypadSelectState.Up;
                if (_keyboard.IsKeyDown(Key.Down))
                    _pressedKeys |= JoypadSelectState.Down;
                if (_keyboard.IsKeyDown(Key.Left))
                    _pressedKeys |= JoypadSelectState.Left;
                if (_keyboard.IsKeyDown(Key.Right))
                    _pressedKeys |= JoypadSelectState.Right;

                if (_keyboard.IsKeyDown(Key.Z))
                    _pressedKeys |= JoypadSelectState.A;
                if (_keyboard.IsKeyDown(Key.X))
                    _pressedKeys |= JoypadSelectState.B;
                if (_keyboard.IsKeyDown(Key.Enter))
                    _pressedKeys |= JoypadSelectState.Start;
                if (_keyboard.IsKeyDown(Key.BackSpace))
                    _pressedKeys |= JoypadSelectState.Select;
            }

            if (_keyboard.IsKeyUp(Key.Up))
                _pressedKeys &= ~JoypadSelectState.Up;
            if (_keyboard.IsKeyUp(Key.Up))
                _pressedKeys &= ~JoypadSelectState.Up;
            if (_keyboard.IsKeyUp(Key.Left))
                _pressedKeys &= ~JoypadSelectState.Left;
            if (_keyboard.IsKeyUp(Key.Right))
                _pressedKeys &= ~JoypadSelectState.Right;

            if (_keyboard.IsKeyUp(Key.Z))
                _pressedKeys &= ~JoypadSelectState.A;
            if (_keyboard.IsKeyUp(Key.X))
                _pressedKeys &= ~JoypadSelectState.B;
            if (_keyboard.IsKeyUp(Key.Enter))
                _pressedKeys &= ~JoypadSelectState.Start;
            if (_keyboard.IsKeyUp(Key.BackSpace))
                _pressedKeys &= ~JoypadSelectState.Select;
        }
    }
}
