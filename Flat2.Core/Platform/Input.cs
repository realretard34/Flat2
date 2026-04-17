using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Numerics;

namespace Flat2.Core.Platform
{
    public class InputMgr : IDisposable
    {
        private readonly IWindow _window;
        private IInputContext? _inputContext;
        private IKeyboard? _keyboard;
        private IMouse? _mouse;
        private IGamepad? _gamepad;
        private readonly HashSet<Key> _keysDown = [];
        private readonly HashSet<MouseButton> _mouseButtonsDown = [];
        private Vector2 _mousePosition;
        private Vector2 _mouseDelta;
        private HashSet<Key> _keysDownPrev = [];
        private HashSet<MouseButton> _mouseButtonsDownPrev = [];
        private Vector2 _mousePositionPrev;
        private readonly HashSet<ButtonName> _gamepadButtonsDown = [];
        private HashSet<ButtonName> _gamepadButtonsDownPrev = [];
        private Vector2 _leftStick, _rightStick;
        private readonly List<InputAction> _registeredActions = [];
        public InputMgr(ref IWindow window)
        {
            _window = window;
            _window.Load += OnWindowLoad;
            _window.Update += OnUpdate;
            
        }
        private void OnWindowLoad()
        {
            _inputContext = _window.CreateInput();
            _keyboard = _inputContext?.Keyboards.FirstOrDefault();
            _mouse = _inputContext?.Mice.FirstOrDefault();
            _gamepad = _inputContext?.Gamepads.FirstOrDefault();
            if (_keyboard != null)
            {
                _keyboard.KeyDown += (kb, key, _) => _keysDown.Add(key);
                _keyboard.KeyUp += (kb, key, _) => _keysDown.Remove(key);
            }
            if (_mouse != null)
            {
                _mouse.MouseDown += (m, btn) => _mouseButtonsDown.Add(btn);
                _mouse.MouseUp += (m, btn) => _mouseButtonsDown.Remove(btn);
                _mouse.MouseMove += (m, pos) => _mousePosition = pos;
            }
            if (_gamepad != null)
            {
                _gamepad.ButtonDown += (gp, btn) => _gamepadButtonsDown.Add(btn.Name);
                _gamepad.ButtonUp += (gp, btn) => _gamepadButtonsDown.Remove(btn.Name);
                _gamepad.ThumbstickMoved += (gp, stick) =>
                {
                    if (stick.Index == 0) _leftStick = new Vector2(stick.X, stick.Y);
                    else if (stick.Index == 1) _rightStick = new Vector2(stick.X, stick.Y);
                };
                _gamepad.TriggerMoved += (gp, trigger) =>
                {
                    if (trigger.Index == 0) GamepadLeftTrigger = trigger.Position;
                    else GamepadRightTrigger = trigger.Position;
                };
            }
            GamepadConnected = _gamepad?.IsConnected ?? false;
        }
        private void OnUpdate(double deltaTime)
        {
            _keysDownPrev = [.. _keysDown];
            _mouseButtonsDownPrev = [.. _mouseButtonsDown];
            _gamepadButtonsDownPrev = [.. _gamepadButtonsDown];
            _mousePositionPrev = _mousePosition;
            _mouseDelta = _mousePosition - _mousePositionPrev;
            if (_gamepad != null)
                GamepadConnected = _gamepad.IsConnected;
            foreach (var action in _registeredActions)
            {
                action.Update();
            }
        }
        internal void RegisterAction(InputAction action)
        {
            if (!_registeredActions.Contains(action))
                _registeredActions.Add(action);
        }
        internal void UnregisterAction(InputAction action)
        {
            _ = _registeredActions.Remove(action);
        }

        public bool IsKeyDown(Key key)
        {
            return _keysDown.Contains(key);
        }

        public bool IsKeyPressed(Key key)
        {
            return _keysDown.Contains(key) && !_keysDownPrev.Contains(key);
        }

        public bool IsKeyReleased(Key key)
        {
            return !_keysDown.Contains(key) && _keysDownPrev.Contains(key);
        }

        public bool IsMouseButtonDown(MouseButton btn)
        {
            return _mouseButtonsDown.Contains(btn);
        }

        public bool IsMouseButtonPressed(MouseButton btn)
        {
            return _mouseButtonsDown.Contains(btn) && !_mouseButtonsDownPrev.Contains(btn);
        }

        public Vector2 MousePosition => _mousePosition;
        public Vector2 MouseDelta => _mouseDelta;
        public bool IsGamepadButtonDown(ButtonName btn)
        {
            return _gamepadButtonsDown.Contains(btn);
        }

        public bool IsGamepadButtonPressed(ButtonName btn)
        {
            return _gamepadButtonsDown.Contains(btn) && !_gamepadButtonsDownPrev.Contains(btn);
        }

        public Vector2 GamepadLeftStick => _leftStick;
        public Vector2 GamepadRightStick => _rightStick;
        public float GamepadLeftTrigger { get; private set; }
        public float GamepadRightTrigger { get; private set; }
        public bool GamepadConnected { get; private set; }
        public void Dispose()
        {
            _inputContext?.Dispose();
            _registeredActions.Clear();
        }
    }
    public class InputAction : IDisposable
    {
        private readonly InputMgr _input;
        private readonly List<Key> _keys = [];
        private readonly List<MouseButton> _mouseButtons = [];
        private readonly List<ButtonName> _gamepadButtons = [];
        private bool _wasDownLastFrame;
        private int _heldFrames;
        public event Action<InputAction, int>? Pressed;
        public event Action<InputAction, int>? Held;
        public event Action<InputAction, int>? Released;
        public InputAction(InputMgr input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _input.RegisterAction(this);
        }
        public InputAction AddKey(Key key) { _keys.Add(key); return this; }
        public InputAction AddMouseButton(MouseButton btn) { _mouseButtons.Add(btn); return this; }
        public InputAction AddGamepadButton(ButtonName btn) { _gamepadButtons.Add(btn); return this; }
        public bool IsDown()
        {
            return _keys.Any(_input.IsKeyDown) ||
            _mouseButtons.Any(_input.IsMouseButtonDown) ||
            _gamepadButtons.Any(_input.IsGamepadButtonDown);
        }

        public bool IsPressed()
        {
            return _keys.Any(_input.IsKeyPressed) ||
                    _mouseButtons.Any(_input.IsMouseButtonPressed) ||
                    _gamepadButtons.Any(_input.IsGamepadButtonPressed);
        }

        internal void Update()
        {
            bool isDownNow = IsDown();
            if (isDownNow)
            {
                _heldFrames++;
                if (!_wasDownLastFrame)
                {
                    _heldFrames = 1;
                    Pressed?.Invoke(this, _heldFrames);
                }
                else
                {
                    Held?.Invoke(this, _heldFrames);
                }
            }
            else if (_wasDownLastFrame)
            {
                Released?.Invoke(this, _heldFrames);
                _heldFrames = 0;
            }
            _wasDownLastFrame = isDownNow;
        }
        public void Dispose()
        {
            _input.UnregisterAction(this);
            Pressed = null;
            Held = null;
            Released = null;
        }
    }
    public class InputAxis
    {
        private readonly InputMgr _input;
        private readonly List<Func<float>> _sources = [];
        public InputAxis(InputMgr input)
        {
            _input = input;
        }

        public InputAxis AddKey(Key negative, Key positive)
        {
            _sources.Add(() => (_input.IsKeyDown(positive) ? 1f : 0f) - (_input.IsKeyDown(negative) ? 1f : 0f));
            return this;
        }
        public InputAxis AddMouseAxis(Func<Vector2, float> selector)
        {
            _sources.Add(() => selector(_input.MouseDelta));
            return this;
        }
        public InputAxis AddGamepadStick(Func<Vector2, float> selector)
        {
            _sources.Add(() => _input.GamepadConnected ? selector(_input.GamepadLeftStick) : 0f);
            return this;
        }
        public float GetValue()
        {
            return _sources.Sum(s => s()).Clamp(-1f, 1f);
        }
    }
    public static class MathExtensions
    {
        public static float Clamp(this float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : value);
        }
    }
}