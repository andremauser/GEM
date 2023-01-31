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
    public enum State
    {
        Idle,
        Hover,
        Press
    }

    internal class MenuButton : BaseControl
    {
        #region Fields
        public event EventHandler OnButtonDown;
        public event EventHandler OnClick;
        public event EventHandler OnHover;
        public event EventHandler OnHoverOut;
        public Dictionary<State, Color> BackColor = new Dictionary<State, Color>();
        public Dictionary<State, Color> TextColor = new Dictionary<State, Color>();
        Dictionary<string, MenuButton> _subMenu;
        MenuButton _parentMenu;
        Panel _subMenuPanel;
        bool _clickStarted = false;
        State _state = State.Idle; // property
        #endregion

        #region Constructors
        public MenuButton(BaseControl parentControl, MenuButton parentMenu, string caption) : base(parentControl)
        {
            _parentMenu = parentMenu;
            Label = AddLabel(caption);
            defaultColors();
            _subMenuPanel = new Panel(this);
        }
        #endregion

        #region Properties
        public Label Label { get; private set; }
        public MenuButton this[string name]
        {
            get { return _subMenu[name]; }
            set { _subMenu[name] = value; }
        }
        public State State 
        {
            get
            {
                return _state;
            }
            private set
            {
                // fire event when state changes
                if (value == State.Hover && _state != State.Hover)
                {
                    if (_clickStarted)
                    {
                        OnClick?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        OnHover?.Invoke(this, EventArgs.Empty);
                    }
                }
                if (value == State.Press && _state != State.Press)
                {
                    OnButtonDown?.Invoke(this, EventArgs.Empty);
                }
                if (value == State.Idle && _state != State.Idle)
                {
                    OnHoverOut?.Invoke(this, EventArgs.Empty);
                }
                _state = value;
                if (Label != null) { Label.TextColor = TextColor[value]; }
            }
        }
        #endregion

        #region Methods
        public override void Update()
        {
            base.Update();

            updateMouse();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, new Rectangle(PosX, PosY, Width, Height), BackColor[State]);
            base.Draw(spriteBatch);
        }

        public void Add(string name)
        {
            if (_subMenu.ContainsKey(name)) return;

            MenuButton button = new MenuButton(_subMenuPanel, this, name);
            _subMenuPanel.Add(button);
            _subMenu.Add(name, button);
        }

        private void defaultColors()
        {
            BackColor[State.Idle] = Color.Gray;
            BackColor[State.Hover] = Color.White;
            BackColor[State.Press] = Color.Blue;

            TextColor[State.Idle] = Color.Black;
            TextColor[State.Hover] = Color.Blue;
            TextColor[State.Press] = Color.White;
        }
        private void updateMouse()
        {
            State nextState = State;
            if (isMouseOver())
            {
                if (Input.IsLeftButtonPressed)
                {
                    nextState = State.Press;
                    if (State == State.Hover) _clickStarted = true;
                }
                else
                {
                    nextState = State.Hover;
                }
            }
            else
            {
                nextState = State.Idle;
            }
            State = nextState;
        }

        private bool isMouseOver()
        {
            int x = Input.MousePosX;
            int y = Input.MousePosY;
            bool hover = false;
            if (x > PosX && x < (PosX + Width) && y > PosY && y < (PosY + Height))
            {
                hover = true;
            }
            return hover;
        }
        #endregion
    }
}
