using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace GEM.Emulation
{
    public enum InputType
    {
        Mouse,
        Keyboard,
        Gamepad
    }
    internal static class Input
    {
        #region Fields
        static Keys[] _lastPressedKeys;
        static bool _wasMouseDown;

        // keyboard events
        public delegate void KeyboardEventHandler(Keys key);
        public static event KeyboardEventHandler OnKeyDown;
        public static event KeyboardEventHandler OnKeyUp;
        // mouse events
        public delegate void MouseEventHanler();
        public static event MouseEventHanler OnMouseDown;
        public static event MouseEventHanler OnMouseUp;

        // gamepad bindings
        const Buttons BUTTON_A =        Buttons.B;
        const Buttons BUTTON_B =        Buttons.A;
        const Buttons BUTTON_START =    Buttons.Start;
        const Buttons BUTTON_SELECT =   Buttons.Back;
        const Buttons BUTTON_UP =       Buttons.DPadUp;
        const Buttons BUTTON_DOWN =     Buttons.DPadDown;
        const Buttons BUTTON_LEFT =     Buttons.DPadLeft;
        const Buttons BUTTON_RIGHT =    Buttons.DPadRight;
        const Buttons BUTTON_MENU =     Buttons.LeftShoulder;
        // keyboard bindings
        const Keys KEY_A =              Keys.X;
        const Keys KEY_B =              Keys.Y;
        const Keys KEY_START =          Keys.Enter;
        const Keys KEY_SELECT =         Keys.Back;
        const Keys KEY_UP =             Keys.Up;
        const Keys KEY_DOWN =           Keys.Down;
        const Keys KEY_LEFT =           Keys.Left;
        const Keys KEY_RIGHT =          Keys.Right;
        const Keys KEY_MENU =           Keys.Tab;

        // TODO: delete
        public static bool IsButton_Color { get; private set; }
        public static bool IsButton_Debug { get; private set; }
        public static bool IsButton_Pause { get; private set; }
        public static bool IsButton_Step { get; private set; }
        public static bool IsButton_Frame { get; private set; }
        public static bool IsButton_Reset { get; private set; }
        public static bool IsButton_Quit { get; private set; }
        public static bool IsButton_Load { get; private set; }
        public static bool IsButton_1 { get; private set; }
        public static bool IsButton_2 { get; private set; }
        public static bool IsButton_3 { get; private set; }
        public static bool IsButton_4 { get; private set; }
        public static bool IsButton_5 { get; private set; }
        #endregion

        #region Properties
        // D-Pad
        public static bool IsButton_Up { get; private set; }
        public static bool IsButton_Down { get; private set; }
        public static bool IsButton_Left { get; private set; }
        public static bool IsButton_Right { get; private set; }

        // Buttons
        public static bool IsButton_A { get; private set; }
        public static bool IsButton_B { get; private set; }
        public static bool IsButton_Start { get; private set; }
        public static bool IsButton_Select { get; private set; }

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

            invokeKeyboardEvents(keyboardState);
            invokeMouseEvents(mouseState);

            // update properties
            IsButton_Up =       gamePadState.IsButtonDown(BUTTON_UP)    ||  keyboardState.IsKeyDown(KEY_UP);
            IsButton_Down =     gamePadState.IsButtonDown(BUTTON_DOWN)  ||  keyboardState.IsKeyDown(KEY_DOWN);
            IsButton_Left =     gamePadState.IsButtonDown(BUTTON_LEFT)  ||  keyboardState.IsKeyDown(KEY_LEFT);
            IsButton_Right =    gamePadState.IsButtonDown(BUTTON_RIGHT) ||  keyboardState.IsKeyDown(KEY_RIGHT);

            IsButton_A =        gamePadState.IsButtonDown(BUTTON_A)     ||  keyboardState.IsKeyDown(KEY_A);
            IsButton_B =        gamePadState.IsButtonDown(BUTTON_B)     ||  keyboardState.IsKeyDown(KEY_B);
            IsButton_Start =    gamePadState.IsButtonDown(BUTTON_START) ||  keyboardState.IsKeyDown(KEY_START);
            IsButton_Select =   gamePadState.IsButtonDown(BUTTON_SELECT)||  keyboardState.IsKeyDown(KEY_SELECT);

            // TODO: delete? 
            MousePosX = mouseState.X;
            MousePosY = mouseState.Y;
            IsLeftButtonPressed = mouseState.LeftButton.HasFlag(ButtonState.Pressed);

            // TODO: delete
            IsButton_Color = gamePadState.Buttons.Y == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.C);
            IsButton_Debug = gamePadState.Buttons.X == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.D);
            IsButton_Pause = gamePadState.Buttons.RightStick == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.P);
            IsButton_Step = gamePadState.Buttons.LeftShoulder == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.S);
            IsButton_Frame = gamePadState.Buttons.RightShoulder == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.F);
            IsButton_Reset = gamePadState.Buttons.RightStick == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.R);
            IsButton_Quit = gamePadState.Buttons.LeftStick == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Q);

            IsButton_Load = keyboardState.IsKeyDown(Keys.L);
            IsButton_1 = keyboardState.IsKeyDown(Keys.D1);
            IsButton_2 = keyboardState.IsKeyDown(Keys.D2);
            IsButton_3 = keyboardState.IsKeyDown(Keys.D3);
            IsButton_4 = keyboardState.IsKeyDown(Keys.D4);
            IsButton_5 = keyboardState.IsKeyDown(Keys.D5);
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
