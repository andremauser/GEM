using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace GEM
{
    /// <summary>
    /// Static snapshot of user input
    /// </summary>
    internal static class Input
    {

        #region Properties

        // Emulator Control
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


        // D-Pad
        public static bool IsButton_Right { get; private set; }
        public static bool IsButton_Left { get; private set; }
        public static bool IsButton_Up { get; private set; }
        public static bool IsButton_Down { get; private set; }

        // Buttons
        public static bool IsButton_A { get; private set; }
        public static bool IsButton_B { get; private set; }
        public static bool IsButton_Select { get; private set; }
        public static bool IsButton_Start { get; private set; }

        // Mouse
        public static int MousePosX { get; private set; }
        public static int MousePosY { get; private set; }
        public static bool IsLeftButtonPressed { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Saves current input state to properties
        /// </summary>
        public static void Update()
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            // Emulator
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

            // D-Pad
            IsButton_Right = gamePadState.DPad.Right == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Right);
            IsButton_Left = gamePadState.DPad.Left == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Left);
            IsButton_Up = gamePadState.DPad.Up == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Up);
            IsButton_Down = gamePadState.DPad.Down == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Down);

            // Buttons
            IsButton_A = gamePadState.Buttons.B == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.X);
            IsButton_B = gamePadState.Buttons.A == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Y);
            IsButton_Select = gamePadState.Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Back);
            IsButton_Start = gamePadState.Buttons.Start == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Enter);

            // Mouse
            MousePosX = mouseState.X;
            MousePosY = mouseState.Y;
            IsLeftButtonPressed = mouseState.LeftButton.HasFlag(ButtonState.Pressed);
        }

        #endregion

    }
}
