using System;
using System.Collections.Generic;
using System.Text;

namespace GEM
{
    public struct Register
    {

        #region Fields

        private byte _value;

        #endregion

        #region Constructors

        public Register(byte value)
        {
            _value = value;
        }

        #endregion

        #region  Properties

        // implicit operators
        public static implicit operator Register(byte value)
        {
            return new Register(value);
        }
        public static implicit operator byte(Register register)
        {
            return register._value;
        }

        // indexer
        public int this[int index]
        {
            get
            {
                return (_value & (int)Math.Pow(2, index)) >> index;
            }
            set
            {
                _value = (byte)((_value & (0xFF - (int)Math.Pow(2, index))) | (value << index));
            }
        }

        #endregion

    }
}
