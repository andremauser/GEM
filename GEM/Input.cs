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

        #endregion

        #region Methods

        /// <summary>
        /// Saves current input state to properties
        /// </summary>
        public static void Update()
        {
            // Emulator
            IsButton_Color = GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.C);
            IsButton_Debug = GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.D);
            IsButton_Pause = GamePad.GetState(PlayerIndex.One).Buttons.RightStick == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.P);
            IsButton_Step = GamePad.GetState(PlayerIndex.One).Buttons.LeftShoulder == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.S);
            IsButton_Frame = GamePad.GetState(PlayerIndex.One).Buttons.RightShoulder == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.F);
            IsButton_Reset = GamePad.GetState(PlayerIndex.One).Buttons.RightStick == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.R);
            IsButton_Quit = GamePad.GetState(PlayerIndex.One).Buttons.LeftStick == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Q);
            IsButton_Load = Keyboard.GetState().IsKeyDown(Keys.L);
            IsButton_1 = Keyboard.GetState().IsKeyDown(Keys.D1);
            IsButton_2 = Keyboard.GetState().IsKeyDown(Keys.D2);
            IsButton_3 = Keyboard.GetState().IsKeyDown(Keys.D3);
            IsButton_4 = Keyboard.GetState().IsKeyDown(Keys.D4);
            IsButton_5 = Keyboard.GetState().IsKeyDown(Keys.D5);

            // D-Pad
            IsButton_Right = GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Right);
            IsButton_Left = GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Left);
            IsButton_Up = GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Up);
            IsButton_Down = GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Down);

            // Buttons
            IsButton_A = GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.X);
            IsButton_B = GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Y);
            IsButton_Select = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Back);
            IsButton_Start = GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter);

            // Mouse
            MousePosX = Mouse.GetState().X;
            MousePosY = Mouse.GetState().Y;
        }

        #endregion

    }
}
