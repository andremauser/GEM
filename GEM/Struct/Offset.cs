using System;
using System.Collections.Generic;
using System.Text;

namespace GEM.Struct
{
    public struct Offset
    {
        // Fields
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        // Constructors
        public Offset(int value)
        {
            Left = value;
            Right = value;
            Top = value;
            Bottom = value;
        }
        public Offset(int horizontal, int vertical)
        {
            Left = horizontal;
            Right = horizontal;
            Top = vertical;
            Bottom = vertical;
        }
        public Offset(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        // Properties
        public int Horizontal
        {
            set
            {
                Left = value;
                Right = value;
            }
        }
        public int Vertical
        {
            set
            {
                Top = value;
                Bottom = value;
            }
        }


        // Implicit
        public static implicit operator Offset(int value)
        {
            return new Offset(value);
        }
        public static implicit operator Offset(int[] value)
        {
            if (value.Length >= 4)
            {
                return new Offset(value[0], value[1], value[2], value[3]);
            }
            else if (value.Length >= 2)
            {
                return new Offset(value[0], value[1]);
            }
            else
            {
                return new Offset();
            }
        }
    }
}
