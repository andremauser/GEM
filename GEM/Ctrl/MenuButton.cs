using GEM.Emu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
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
    public enum MenuType
    {
        Click,
        Hover
    }

    internal class MenuButton : BaseControl
    {
        #region Fields
        // constants
        const int DEFAULT_WIDTH = 100;
        const int DEFAULT_HEIGHT = 60;
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
        public MenuButton(BaseControl parentControl, MenuButton parentMenu, string caption, MenuType menuType) : base(parentControl)
        {
            _parentMenu = parentMenu;
            Label = AddLabel(caption);
            Panel = AddPanel();
            Panel.Visible = false;
            switch (menuType)
            {
                case MenuType.Click:
                    OnClick += ToggleMenu;
                    OnHover += OpenIfSideOpen;
                    break;
                case MenuType.Hover:
                    OnHover += Open;
                    break;
                default:
                    break;
            }
            OnHover += CloseSideMenus;
            // default values
            applyDefaultColors();
            Width = DEFAULT_WIDTH;
            Height= DEFAULT_HEIGHT;
            // set submenu anchor point (size of panel is 0)
            Panel.HorizontalAlign = Align.Right;
            Panel.VerticalAlign = Align.Top;
        }
        #endregion

        #region Properties
        public Label Label { get; private set; }
        public Panel Panel { get; private set; }
        public MenuButton this[string name]
        {
            // submenu access by name
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
                // fire menu button events on state change
                // hover + click
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
                // press
                if (value == State.Press && _state != State.Press)
                {
                    OnPress?.Invoke(this, EventArgs.Empty);
                }
                // hover out
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
            Color color = BackColor[State];
            // highlight open menu button with hover preset
            if ( Panel.Visible && _subMenu.Count > 0 && State == State.Idle)
            {
                color = BackColor[State.Hover];
                Label.TextColor = TextColor[State.Hover];
            }

            spriteBatch.Draw(_pixel, new Rectangle(PosX, PosY, Width, Height), color);
            base.Draw(spriteBatch);
        }

        public void AddMenu(string name, MenuButton button)
        {
            if (_subMenu.ContainsKey(name)) return;

            Panel.Add(button);
            _subMenu.Add(name, button);
        }
        public void AddMenu(string name, MenuType menuType, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT)
        {
            AddMenu(name, new MenuButton(Panel, this, name, menuType) { Width = width, Height = height});
        }

        public void ToggleMenu<EventArgs>(Object sender, EventArgs e)
        {
            if (Panel.Visible)
            {
                Close(sender, e);
            }
            else
            {
                Open(sender, e);
            }
        }
        public void Close<EventArgs>(Object sender, EventArgs e)
        {
            Panel.Visible = false;
            foreach (MenuButton button in _subMenu.Values)
            {
                button.Panel.Visible = false;
            }
        }
        public void Open<EventArgs>(Object sender, EventArgs e)
        {
            Panel.Visible = true;
        }
        public void OpenIfSideOpen<EventArgs>(Object sender, EventArgs e)
        {
            if (_parentMenu == null) return;

            bool open = false;
            foreach (MenuButton button in _parentMenu._subMenu.Values)
            {
                open |= button.Panel.Visible;
            }
            if (open)
            {
                Open(sender, e);
            }
        }
        public void CloseSideMenus<EventArgs>(Object sender, EventArgs e)
        {
            if (_parentMenu == null) return;

            foreach (MenuButton button in _parentMenu._subMenu.Values)
            {
                if (button != this)
                {
                    button.Close(sender, e);
                }
            }
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
        private bool isClickStartedR()
        {
            bool started = _clickStarted;
            foreach (MenuButton button in _subMenu.Values)
            {
                started |= button.isClickStartedR();
            }
            return started;
        }

        // mouse
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
        private bool isMouseOverR()
        {
            // recursive hover check
            bool hover = isMouseOver();
            if (Panel.Visible)
            {
                foreach (MenuButton sub in _subMenu.Values)
                {
                    hover |= sub.isMouseOverR();
                }
            }
            return hover;
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
            if (_parentMenu == null)
            {
                if (!isMouseOverR() && Input.IsLeftButtonPressed && !isClickStartedR())
                {
                    Close(null, EventArgs.Empty);
                }
            }
            State = nextState;
        }
        #endregion
    }
}
