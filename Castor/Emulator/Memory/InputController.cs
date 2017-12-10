using OpenTK.Input;

namespace Castor.Emulator.Memory
{
    public class InputController
    {
        JoypadSelectState _selected;
        GamePadState _gamepad;
        KeyboardState _keyboard;
        bool usingGamepad;
        int _gamepadIndex;

        public InputController()
        {
            _selected = JoypadSelectState.None;
            usingGamepad = false;
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
