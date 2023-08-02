using GEM.Emulation;
using Microsoft.Xna.Framework;
using System;

namespace GEM.Menu
{
    public enum Element
    {
        Background,
        Foreground,
        Border
    }

    internal class Style
    {
        #region Fields
        Random _rand = new Random();
        Emulator _emu = null;
        Color[] _palette;

        // for each element(3) and each state(4)
        Color[,] _colorFixed  = new Color[3, 4];
        int[,]   _colorIndex  = new int[3, 4];
        float[,] _colorAlpha  = new float[3, 4];
        #endregion

        #region Constructors
        public Style()
        {
            defaultValues();
        }
        public Style(Emulator emu)
        {
            defaultValues();
            // TODO: Better: Pass event as parameter (did not work)
            _emu = emu;
            _emu.OnPaletteChange += updatePalette;
        }
        public Style(Style copyFrom)
        {
            _palette    = (Color[]) copyFrom._palette.Clone();
            _colorFixed = (Color[,])copyFrom._colorFixed.Clone();
            _colorIndex = (int[,])  copyFrom._colorIndex.Clone();
            _colorAlpha = (float[,])copyFrom._colorAlpha.Clone();
            
            if (copyFrom._emu != null)
            {
                _emu = copyFrom._emu;
                _emu.OnPaletteChange += updatePalette;
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void defaultValues()
        {
            // random palette
            _palette = new Color[]
            {
                new Color(_rand.Next(255), _rand.Next(255), _rand.Next(255)),
                new Color(_rand.Next(255), _rand.Next(255), _rand.Next(255)),
                new Color(_rand.Next(255), _rand.Next(255), _rand.Next(255)),
                new Color(_rand.Next(255), _rand.Next(255), _rand.Next(255))
            };
            // random fixed colors
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _colorFixed[i, j] = new Color(_rand.Next(255), _rand.Next(255), _rand.Next(255));
                    _colorIndex[i, j] = -1;
                    _colorAlpha[i, j] = 1.0f;
                }
            };
        }
        public void updatePalette(Color[] newPalette)
        {
            _palette = newPalette;
        }

        public Color GetColor(Element element, State state)
        {
            int i = (int)element;
            int j = (int)state;
            return new Color(_colorIndex[i, j] >= 0 ? _palette[_colorIndex[i, j]] : _colorFixed[i, j], _colorAlpha[i, j]);
        }
        public void SetColor(Element element, State state, Color color)
        {
            int i = (int)element;
            int j = (int)state;
            _colorFixed[i, j] = color;
            _colorIndex[i, j] = -1;
        }
        public void SetColor(Element element, State state, Color color, float alpha)
        {
            SetColor(element, state, color);
            SetAlpha(element, state, alpha);
        }
        public void SetColor(Element element, State state, int paletteIndex)
        {
            int i = (int)element;
            int j = (int)state;
            _colorIndex[i, j] = paletteIndex;
        }
        public void SetColor(Element element, State state, int paletteIndex, float alpha)
        {
            SetColor(element, state, paletteIndex);
            SetAlpha(element, state, alpha);
        }
        public void SetAlpha(Element element, State state, float alpha)
        {
            int i = (int)element;
            int j = (int)state;
            _colorAlpha[i, j] = alpha;
        }
        public Color BackColor(State state)
        {
            return GetColor(Element.Background, state);
        }
        public Color ForeColor(State state)
        {
            return GetColor(Element.Foreground, state);
        }
        public Color BorderColor(State state)
        {
            return GetColor(Element.Border, state);
        }
        #endregion
    }
}
