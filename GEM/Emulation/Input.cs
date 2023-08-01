using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static Vector2 LastMousePosition { get; set; }
        public static Vector2 CurrentMousePosition
        {
            get
            {
                return new Vector2(MousePosX, MousePosY);
            }
        }
        public static bool IsMouseVisible
        {
            get
            {
                return Game1._Instance.IsMouseVisible;
            }
            set
            {
                Game1._Instance.IsMouseVisible = value;
            }
        }
        public static TouchCollection TouchCollection { get; private set; }
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
            // reactivate mouse
            if (!IsMouseVisible)
            {
                if (Vector2.Distance(CurrentMousePosition, LastMousePosition) > 10)
                {
                    IsMouseVisible = true;
                }
            }

            invokeKeyboardEvents(keyboardState);
            invokeGamepadEvents(gamePadState);
            invokeMouseEvents(mouseState);

            TouchCollection = TouchPanel.GetState();
        }

        static void invokeKeyboardEvents(KeyboardState keyboardState)
        {
            // keyboard events
            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            if (pressedKeys.Count() > 0)
            {
                // hide mouse
                IsMouseVisible = false;
                LastMousePosition = CurrentMousePosition;
            }
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
            if (pressedButtons.Count() > 0)
            {
                // hide mouse
                IsMouseVisible = false;
                LastMousePosition = CurrentMousePosition;
            }
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

            // left thumbstick triggers dpad
            float x = gamepadState.ThumbSticks.Left.X;
            float y = gamepadState.ThumbSticks.Left.Y;
            float distance = gamepadState.ThumbSticks.Left.Length();
            float threshhold = 0.5f;
            float threshholdProjection = (float)(threshhold * Math.Sin(2 * Math.PI / 16f));
            if (distance >= threshhold)
            {
                if (x >= threshholdProjection && !pressedButtons.Contains(Buttons.DPadRight))
                {
                    // right
                    pressedButtons.Add(Buttons.DPadRight);
                }
                if (x <= -threshholdProjection && !pressedButtons.Contains(Buttons.DPadLeft))
                {
                    // left
                    pressedButtons.Add(Buttons.DPadLeft);
                }
                if (y >= threshholdProjection && !pressedButtons.Contains(Buttons.DPadUp))
                {
                    // up
                    pressedButtons.Add(Buttons.DPadUp);
                }
                if (y <= -threshholdProjection && !pressedButtons.Contains(Buttons.DPadDown))
                {
                    // down
                    pressedButtons.Add(Buttons.DPadDown);
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
