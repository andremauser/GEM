using GEM.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace GEM.Emulation
{
    internal static class Input
    {
        #region Fields
        static Keys[] _lastPressedKeys;
        static Buttons[] _lastPressedButtons;
        static bool _wasMouseDown;

        // keyboard events
        public delegate void KeyboardEventHandler(Keys key);
        public static event KeyboardEventHandler OnKeyDown;
        public static event KeyboardEventHandler OnKeyUp;
        // gamepad events
        public delegate void GamepadEventHandler(Buttons button);
        public static event GamepadEventHandler OnButtonDown;
        public static event GamepadEventHandler OnButtonUp;
        // mouse events
        public delegate void MouseEventHanler();
        public static event MouseEventHanler OnMouseDown;
        public static event MouseEventHanler OnMouseUp;
        #endregion

        #region Properties
        // Mouse
        public static int MousePosX { get; private set; }
        public static int MousePosY { get; private set; }
        public static bool IsLeftButtonPressed { get; private set; }
        #endregion

        #region Methods
        public static void Update()
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            MousePosX = mouseState.X;
            MousePosY = mouseState.Y;
            IsLeftButtonPressed = mouseState.LeftButton.HasFlag(ButtonState.Pressed);

            invokeKeyboardEvents(keyboardState);
            invokeGamepadEvents(gamePadState);
            invokeMouseEvents(mouseState);
        }

        static void invokeKeyboardEvents(KeyboardState keyboardState)
        {
            // keyboard events
            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            if (_lastPressedKeys != null)
            {
                // key down
                foreach (Keys key in pressedKeys)
                {
                    bool newPress = true;
                    foreach (Keys lastKey in _lastPressedKeys)
                    {
                        if (key == lastKey) newPress = false;
                    }
                    if (newPress) OnKeyDown?.Invoke(key);
                }
                // key up
                foreach (Keys lastKey in _lastPressedKeys)
                {
                    bool endPress = true;
                    foreach (Keys key in pressedKeys)
                    {
                        if (key == lastKey) endPress = false;
                    }
                    if (endPress) OnKeyUp?.Invoke(lastKey);
                }
            }
            _lastPressedKeys = pressedKeys;
        }
        static void invokeGamepadEvents(GamePadState gamepadState)
        {

            // gamepad events
            Buttons[] pressedButtons = GetPressedButtons(gamepadState);
            if (_lastPressedButtons != null)
            {
                // button down
                foreach (Buttons btn in pressedButtons)
                {
                    bool newPress = true;
                    foreach (Buttons lastBtn in _lastPressedButtons)
                    {
                        if (btn == lastBtn) newPress = false;
                    }
                    if (newPress) OnButtonDown?.Invoke(btn);
                }
                // button up
                foreach (Buttons lastBtn in _lastPressedButtons)
                {
                    bool endPress = true;
                    foreach (Buttons btn in pressedButtons)
                    {
                        if (btn == lastBtn) endPress = false;
                    }
                    if (endPress) OnButtonUp?.Invoke(lastBtn);
                }
            }
            _lastPressedButtons = pressedButtons;
        }
        static Buttons[] GetPressedButtons(GamePadState gamepadState)
        {
            List<Buttons> buttons = new List<Buttons>()
            {
                Buttons.A,
                Buttons.B,
                Buttons.X,
                Buttons.Y,
                Buttons.Start,
                Buttons.Back,
                Buttons.DPadLeft,
                Buttons.DPadRight,
                Buttons.DPadUp,
                Buttons.DPadDown,
                Buttons.LeftShoulder,
                Buttons.RightShoulder
            };
            List<Buttons> pressedButtons = new List<Buttons>();

            foreach (Buttons btn in buttons)
            {
                if (gamepadState.IsButtonDown(btn))
                {
                    pressedButtons.Add(btn);
                }
            }

            return pressedButtons.ToArray();
        }
        static void invokeMouseEvents(MouseState mouseState)
        {
            // mouse events

            // mouse down
            if (mouseState.LeftButton.HasFlag(ButtonState.Pressed) && !_wasMouseDown)
            {
                _wasMouseDown = true;
                OnMouseDown?.Invoke();
            }
            // mouse up
            if (!mouseState.LeftButton.HasFlag(ButtonState.Pressed) && _wasMouseDown)
            {
                _wasMouseDown = false;
                OnMouseUp?.Invoke();
            }
        }

        #endregion

    }
}
