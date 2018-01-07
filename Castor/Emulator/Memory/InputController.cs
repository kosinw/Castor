using OpenTK.Input;

namespace Castor.Emulator.Memory
{
    public class InputController
    {
        JoypadSelectState _pressedKeysDirectional;
        JoypadSelectState _pressedKeysButtons;

        JoypadSelectState _previousKeysDirectional;
        JoypadSelectState _previousKeysButtons;

        private JoypadSelectState Directional
        {
            get => _pressedKeysDirectional;
            set
            {
                var previousState = _pressedKeysDirectional;
                _pressedKeysDirectional = value;

                if (previousState == JoypadSelectState.None
                    && _selectedKeys != SelectedKeys.None
                    && _previousKeysDirectional != _pressedKeysDirectional)
                {
                    _d.IRQ.RequestInterrupt(InterruptFlags.Joypad);
                }
            }
        }

        private JoypadSelectState Buttons
        {
            get => _pressedKeysButtons;
            set
            {
                var previousState = _pressedKeysButtons;
                _pressedKeysButtons = value;

                if (previousState == JoypadSelectState.None
                    && _selectedKeys != SelectedKeys.None
                    && _previousKeysButtons != _pressedKeysButtons)
                {
                    _d.IRQ.RequestInterrupt(InterruptFlags.Joypad);
                }
            }
        }

        GamePadState _gamepad;
        KeyboardState _keyboard;
        SelectedKeys _selectedKeys;
        Device _d;
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
                    body |= (byte)(_pressedKeysButtons | JoypadSelectState.A);
                    body |= (byte)(_pressedKeysButtons | JoypadSelectState.B);
                    body |= (byte)(_pressedKeysButtons | JoypadSelectState.Select);
                    body |= (byte)(_pressedKeysButtons | JoypadSelectState.Start);
                }

                if (_selectedKeys.HasFlag(SelectedKeys.Direction))
                {
                    body |= (byte)(_pressedKeysDirectional | JoypadSelectState.Down);
                    body |= (byte)(_pressedKeysDirectional | JoypadSelectState.Up);
                    body |= (byte)(_pressedKeysDirectional | JoypadSelectState.Left);
                    body |= (byte)(_pressedKeysDirectional | JoypadSelectState.Right);
                }

                return (byte)~(header | body);
            }

            set
            {
                byte header = (byte)(~(value >> 4) & 0b11);

                _selectedKeys |= (SelectedKeys)header;
            }
        }

        public InputController(Device d)
        {
            _d = d;
            _pressedKeysDirectional = JoypadSelectState.None;
            _pressedKeysButtons = JoypadSelectState.None;
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
                _gamepadIndex--;
            }
        }

        public void Step()
        {
            _previousKeysButtons = _pressedKeysButtons;
            _previousKeysDirectional = _pressedKeysDirectional;

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
                    Directional |= JoypadSelectState.Up;
                if (_keyboard.IsKeyDown(Key.Down))
                    Directional |= JoypadSelectState.Down;
                if (_keyboard.IsKeyDown(Key.Left))
                    Directional |= JoypadSelectState.Left;
                if (_keyboard.IsKeyDown(Key.Right))
                    Directional |= JoypadSelectState.Right;

                if (_keyboard.IsKeyDown(Key.Z))
                    Buttons |= JoypadSelectState.A;
                if (_keyboard.IsKeyDown(Key.X))
                    Buttons |= JoypadSelectState.B;
                if (_keyboard.IsKeyDown(Key.Enter))
                    Buttons |= JoypadSelectState.Start;
                if (_keyboard.IsKeyDown(Key.BackSpace))
                    Buttons |= JoypadSelectState.Select;
            }

            if (_selectedKeys.HasFlag(SelectedKeys.Direction))
            {
                if (_keyboard.IsKeyUp(Key.Up))
                    Directional &= ~JoypadSelectState.Up;
                if (_keyboard.IsKeyUp(Key.Down))
                    Directional &= ~JoypadSelectState.Down;
                if (_keyboard.IsKeyUp(Key.Left))
                    Directional &= ~JoypadSelectState.Left;
                if (_keyboard.IsKeyUp(Key.Right))
                    Directional &= ~JoypadSelectState.Right;
            }

            if (_selectedKeys.HasFlag(SelectedKeys.Buttons))
            {
                if (_keyboard.IsKeyUp(Key.Z))
                    Buttons &= ~JoypadSelectState.A;
                if (_keyboard.IsKeyUp(Key.X))
                    Buttons &= ~JoypadSelectState.B;
                if (_keyboard.IsKeyUp(Key.Enter))
                    Buttons &= ~JoypadSelectState.Start;
                if (_keyboard.IsKeyUp(Key.BackSpace))
                    Buttons &= ~JoypadSelectState.Select;
            }
        }
    }
}
