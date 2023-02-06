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
        // menu button events
        public event EventHandler OnPress;
        public event EventHandler OnClick;
        public event EventHandler OnHover;
        public event EventHandler OnHoverOut;
        // menu button colors
        public Dictionary<State, Color> BackColor = new Dictionary<State, Color>();
        public Dictionary<State, Color> TextColor = new Dictionary<State, Color>();
        // menu structure
        Dictionary<string, MenuButton> _subMenu = new Dictionary<string, MenuButton>();
        MenuButton _parentMenu;
        // property fields
        State _state = State.Idle;
        // button functionality
        bool _clickStarted = false;
        #endregion

        #region Constructors
        public MenuButton(BaseControl parentControl, MenuButton parentMenu, string caption) : base(parentControl)
        {
            _parentMenu = parentMenu;
            Label = AddLabel(caption);
            Panel = AddPanel();
            // set submenu anchor point (size of panel is 0)
            Panel.HorizontalAlign = Align.Right;
            Panel.VerticalAlign= Align.Top;
            // default values
            applyDefaultColors();
            Width = 100;
            Height= 50;
        }
        #endregion

        #region Properties
        public Label Label { get; private set; }
        public Panel Panel { get; private set; }
        public MenuButton this[string name]
        {
            // submenu acces by name
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
                // fire menu button event on state change
                if (value == State.Hover && _state != State.Hover)
                {
                    if (_clickStarted)
                    {
                        OnClick?.Invoke(this, EventArgs.Empty);
                        _clickStarted= false;
                    }
                    else
                    {
                        OnHover?.Invoke(this, EventArgs.Empty);
                    }
                }
                if (value == State.Press && _state != State.Press)
                {
                    OnPress?.Invoke(this, EventArgs.Empty);
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

        public void AddMenu(string name, MenuButton button)
        {
            if (_subMenu.ContainsKey(name)) return;

            Panel.Add(button);
            _subMenu.Add(name, button);
        }
        public void AddMenu(string name)
        {
            AddMenu(name, new MenuButton(Panel, this, name));
        }

        public void ToggleMenu<EventArgs>(Object sender, EventArgs e)
        {
            Panel.Visible = !Panel.Visible;
        }
        // private helper methods
        private void applyDefaultColors()
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
                    if (State == State.Hover || _clickStarted)
                    {
                        nextState = State.Press;
                        _clickStarted = true;
                    }
                }
                else
                {
                    nextState = State.Hover;
                }
            }
            else
            {
                nextState = State.Idle;
                if (!Input.IsLeftButtonPressed) _clickStarted = false;
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
