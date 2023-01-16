using GEM.Emu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    public enum Direction
    { 
        Left, 
        Right, 
        Up, 
        Down 
    }

    internal class Submenu : BaseControl
    {
        protected Direction _direction;

        protected Image _root; // add _root as _controls[0] !

        public Submenu(BaseControl parent, Emulator emulator, Direction direction) : base(parent, emulator)
        {
            _direction = direction;
            _backColorIdle = Color.Transparent;
            _backColorHover = Color.DarkMagenta;
            _backColorPress = Color.White;
        }

        public override void Update()
        {
            switch (_customState)
            {
                case CustomState.Collapsed:
                    _backColorPress = Color.White;
                    break;
                case CustomState.Expanded:
                    _backColorPress = Color.DarkMagenta;
                    break;
                default:
                    break;
            }

            // arrange embedded controls
            switch (_direction)
            {
                case Direction.Left:
                    // TODO
                    break;
                case Direction.Right:
                    int top = 0;
                    for (int i = 1; i < _controls.Count; i++)
                    {
                        _controls[i].Left = _root.Width;
                        _controls[i].Top = top;
                        top += _controls[i].Height;
                    }
                    break;
                case Direction.Up:
                    // TODO
                    break;
                case Direction.Down:
                    // TODO
                    break;
                default:
                    break;
            }
            //_root.Update();
            base.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            //_root.Draw(spriteBatch);
        }

        public override int Width 
        { 
            get
            {
                int width = 0;
                switch (_customState)
                {
                    case CustomState.Collapsed:
                        width = _root.Width;
                        break;
                    case CustomState.Expanded:
                        int maxControlWidth = 0;
                        for (int i = 1; i < _controls.Count; i++)
                        {
                            maxControlWidth = Math.Max(_controls[i].Width, maxControlWidth);
                        }
                        switch (_direction)
                        {
                            case Direction.Left:
                            case Direction.Right:
                                width = _root.Width + maxControlWidth;
                                break;
                            case Direction.Up:
                            case Direction.Down:
                                width = Math.Max(_root.Width, maxControlWidth);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                return width;
            }
            set => base.Width = value; 
        }

        public override int Height
        {
            get
            {
                int height = 0;
                switch (_customState)
                {
                    case CustomState.Collapsed:
                        height = _root.Height;
                        break;
                    case CustomState.Expanded:
                        int totalHeight = 0;
                        for (int i = 1; i < _controls.Count; i++)
                        {
                            totalHeight += _controls[i].Height;
                        }
                        switch (_direction)
                        {
                            case Direction.Left:
                            case Direction.Right:
                                height = Math.Max(_root.Height, totalHeight);
                                break;
                            case Direction.Up:
                            case Direction.Down:
                                height = _root.Height + totalHeight;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                return height;
            }
            set => base.Width = value;
        }

        internal override void onClick()
        {
            for (int i = 1; i < _controls.Count; i++)
            {
                _controls[i].Visible = true;
            }
            CustomState = CustomState.Expanded;
            base.onClick();
        }

        internal override void onHoverOut()
        {
            for (int i = 1; i < _controls.Count; i++)
            {
                _controls[i].Visible = false;
            }
            CustomState = CustomState.Collapsed;
            base.onHoverOut();
        }

    }
}
