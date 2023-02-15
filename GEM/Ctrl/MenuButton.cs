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
        Press,
        Disabled
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
        const int DEFAULT_WIDTH = 150;
        const int DEFAULT_HEIGHT = 60;
        // menu button events
        public delegate void MenuEventHandler();
        public event MenuEventHandler OnPress;
        public event MenuEventHandler OnClick;
        public event MenuEventHandler OnHover;
        public event MenuEventHandler OnHoverOut;
        public event MenuEventHandler OnOpen;
        public event MenuEventHandler OnClose;
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
        public bool Enabled = true;
        #endregion

        #region Constructors
        public MenuButton(BaseControl parentControl, MenuButton parentMenu, string caption, MenuType menuType, string image = null) : base(parentControl)
        {
            _parentMenu = parentMenu;
            Caption = AddLabel(caption);
            if (image != null)
            {
                Image = AddImage(image);
                Caption.Caption = "";
            }
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
                    OnClick += ToggleMenu;
                    break;
                default:
                    break;
            }
            OnHover += CloseSideMenus;
            // default values
            applyDefaultColors();
            Width = DEFAULT_WIDTH;
            Height = DEFAULT_HEIGHT;
            // set submenu anchor point (size of panel is 0)
            Panel.HorizontalAlign = Align.Right;
            Panel.VerticalAlign = Align.Top;
        }
        #endregion

        #region Properties
        public Label Caption { get; private set; }
        public Image Image { get; private set; }
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
                        OnClick?.Invoke();
                        _clickStarted= false;
                    }
                    else
                    {
                        OnHover?.Invoke();
                    }
                }
                // press
                if (value == State.Press && _state != State.Press)
                {
                    OnPress?.Invoke();
                }
                // hover out
                if (value == State.Idle && _state != State.Idle)
                {
                    OnHoverOut?.Invoke();
                }
                _state = value;
                if (Caption != null) Caption.TextColor = TextColor[value];
                if (Image != null) Image.ImageColor = TextColor[value];
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
            // highlight open menuButton
            Color color = BackColor[State];
            if (Panel.Visible && _subMenu.Count > 0 && State == State.Idle)
            {
                color = BackColor[State.Hover];
                Caption.TextColor = TextColor[State.Hover];
                if (Image != null) Image.ImageColor = TextColor[State.Hover];
            }
            // draw box
            spriteBatch.Draw(_pixel, new Rectangle(PosX, PosY, Width, Height), color);
            // draw label/image and submenu
            base.Draw(spriteBatch);
        }

        public MenuButton AddMenu(string name, MenuButton button)
        {
            if (_subMenu.ContainsKey(name)) return null;

            Panel.Add(button);
            _subMenu.Add(name, button);
            return button;
        }
        public MenuButton AddMenu(string name, MenuType menuType = MenuType.Hover, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT, string image = null)
        {
            return AddMenu(name, new MenuButton(Panel, this, name, menuType, image) { Width = width, Height = height});
        }

        // event handler
        public void ToggleMenu()
        {
            if (Panel.Visible)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
        public void Close()
        {
            Panel.Visible = false;
            foreach (MenuButton button in _subMenu.Values)
            {
                button.Panel.Visible = false;
            }
            OnClose?.Invoke();
        }
        public void Open()
        {
            Panel.Visible = true;
            OnOpen?.Invoke();
        }
        public void OpenIfSideOpen()
        {
            if (_parentMenu == null) return;

            bool open = false;
            foreach (MenuButton button in _parentMenu._subMenu.Values)
            {
                open |= button.Panel.Visible;
            }
            if (open)
            {
                Open();
            }
        }
        public void CloseSideMenus()
        {
            if (_parentMenu == null) return;

            foreach (MenuButton button in _parentMenu._subMenu.Values)
            {
                if (button != this)
                {
                    button.Close();
                }
            }
        }

        // private helper methods
        private void applyDefaultColors()
        {
            BackColor[State.Idle] = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            BackColor[State.Hover] = Color.MediumSpringGreen;
            BackColor[State.Press] = Color.MediumVioletRed;
            BackColor[State.Disabled] = BackColor[State.Idle];

            TextColor[State.Idle] = Color.White;
            TextColor[State.Hover] = Color.Black;
            TextColor[State.Press] = Color.White;
            TextColor[State.Disabled] = Color.Gray;
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
            if (!Enabled)
            {
                State = State.Disabled;
                return;
            }
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
                    Close();
                }
            }
            State = nextState;
        }
        #endregion
    }
}
