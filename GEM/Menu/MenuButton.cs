using GEM.Emulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GEM.Menu
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
        Hover,
        StandAlone
    }

    internal class MenuButton : BaseControl
    {
        #region Fields
        // constants
        const int DEFAULT_WIDTH = 150;
        const int DEFAULT_HEIGHT = 60;
        // simple event handler
        public delegate void MenuEventHandler();
        // menu button events
        public event MenuEventHandler OnPress;
        public event MenuEventHandler OnRelease;
        public event MenuEventHandler OnClick;
        public event MenuEventHandler OnHover;
        public event MenuEventHandler OnHoverOut;
        // menu structure events
        public event MenuEventHandler OnOpen;
        public event MenuEventHandler OnClose;
        // menu button colors
        public Dictionary<State, Color> BackColor = new Dictionary<State, Color>();
        public Dictionary<State, Color> ForeColor = new Dictionary<State, Color>();
        // menu structure
        Dictionary<string, MenuButton> _subMenu = new Dictionary<string, MenuButton>();
        MenuButton _parentMenu;
        int _menuIndex;
        // property fields
        State _state = State.Idle;
        Keys _keyBinding;
        Buttons _buttonBinding;
        static MenuButton _fokus;
        // button functionality
        bool _clickStarted = false;
        public bool Enabled = true;
        State _gamepadRequest;
        State _keyboardRequest;
        State _mouseRequest;
        #endregion

        #region Constructors
        public MenuButton(BaseControl parentControl, MenuButton parentMenu, string caption, MenuType menuType, string image = null) : base(parentControl)
        {
            _parentMenu = parentMenu;
            Label = AddLabel(caption);
            if (image != null)
            {
                Image = AddImage(image);
                Label.Caption = "";
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
            OnHover += CloseSubSubmenus;

            // bind mouse events
            Input.OnMouseDown += MouseDownHandler;
            Input.OnMouseUp += MouseUpHandler;

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
        public Label Label { get; private set; }
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

                // hover
                if (value == State.Hover && _state == State.Idle)
                {
                    OnHover?.Invoke();
                }
                // press
                if (value == State.Press && _state != State.Press)
                {
                    _clickStarted = true;
                    OnPress?.Invoke();
                }
                // release
                if (value != State.Press && _state == State.Press)
                {
                    OnRelease?.Invoke();
                }
                // click
                if (value != State.Press && _state == State.Press && _clickStarted)
                {
                    OnClick?.Invoke();
                    _clickStarted = false;
                }
                // hover out
                if (value == State.Idle && _state != State.Idle)
                {
                    OnHoverOut?.Invoke();
                }
                _state = value;
                if (Label != null) Label.ForeColor = ForeColor[value];
                if (Image != null) Image.ForeColor = ForeColor[value];
            }
        }
        public Keys KeyBinding
        {
            get
            {
                return _keyBinding;
            }
            set
            {
                _keyBinding = value;
                Input.OnKeyDown += KeyDownHandler;
                Input.OnKeyUp += KeyUpHandler;
            }
        }
        public Buttons BtnBinding
        {
            get
            {
                return _buttonBinding;
            }
            set
            {
                _buttonBinding = value;
                Input.OnButtonDown += ButtonDownHandler;
                Input.OnButtonUp += ButtonUpHandler;
            }
        }
        public static MenuButton Fokus
        {
            get
            {
                return _fokus;
            }
            set
            {
                if (_fokus != null) Input.OnKeyDown -= _fokus.NavigationHandler;
                _fokus = value;
                if (_fokus != null) Input.OnKeyDown += _fokus.NavigationHandler;
            }
        }
        #endregion

        #region Methods
        public override void Update()
        {
            // mouse hover
            if (isMouseOver() && _mouseRequest == State.Idle && !Input.IsLeftButtonPressed)
            {
                _mouseRequest = State.Hover;
            }
            if (!isMouseOver() && _mouseRequest != State.Press)
            {
                _mouseRequest = State.Idle;
            }

            resolveStateRequests();

            if (!Visible) return;
            base.Update();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            // highlight button when submenu is visible
            Color color = BackColor[State];
            if (Panel.Visible && _subMenu.Count > 0 && State == State.Idle)
            {
                color = BackColor[State.Hover];
                Label.ForeColor = ForeColor[State.Hover];
                if (Image != null) Image.ForeColor = ForeColor[State.Hover];
            }
            // draw box
            spriteBatch.Draw(_pixel, new Rectangle(PosX, PosY, Width, Height), color);
            // draw label, image and submenu
            base.Draw(spriteBatch);
        }

        // add submenu
        public MenuButton AddMenu(string name, MenuButton button)
        {
            // check for duplicate name
            if (_subMenu.ContainsKey(name)) return null;

            // add to embedded controls
            Panel.Add(button);

            // add to menu structure
            button._menuIndex = _subMenu.Count; // index for menu navigation
            _subMenu.Add(name, button); 

            return button;
        }
        public MenuButton AddMenu(string name, MenuType menuType = MenuType.Hover, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT, string image = null)
        {
            return AddMenu(name, new MenuButton(Panel, this, name, menuType, image) { Width = width, Height = height});
        }

        // menu event handler
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
                // close all submenus
                button.Close();
            }
            OnClose?.Invoke();
        }
        public void Open()
        {
            Fokus = this;
            Panel.Visible = true;
            OnOpen?.Invoke();
        }
        public void OpenIfSideOpen()
        {
            // used for open menu on hover when another menu on same level is already opened

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
            // used for closing other menus on same level when menu opened

            if (_parentMenu == null) return;

            foreach (MenuButton button in _parentMenu._subMenu.Values)
            {
                if (button != this)
                {
                    button.Close();
                }
            }
        }
        public void CloseSubSubmenus()
        {
            foreach (MenuButton button in _subMenu.Values)
            {
                button.Close();
            }
        }

        // keyboard event handler
        public void KeyDownHandler(Keys key)
        {
            if (key == KeyBinding)
            {
                _keyboardRequest = State.Press;
            }
        }
        public void KeyUpHandler(Keys key)
        {
            if (key == KeyBinding)
            {
                _keyboardRequest = State.Idle;
                _clickStarted = true; // keep clickStarted even when mouse click outside
            }
        }

        // gamepad event handler
        public void ButtonDownHandler(Buttons button)
        {
            if (button == BtnBinding)
            {
                _gamepadRequest = State.Press;
            }
        }
        public void ButtonUpHandler(Buttons key)
        {
            if (key == BtnBinding)
            {
                _gamepadRequest = State.Idle;
                _clickStarted = true; // keep clickStarted even when mouse click outside
            }
        }

        // mouse event handler
        public void MouseDownHandler()
        {
            if (isMouseOver())
            {
                _mouseRequest = State.Press;
                _clickStarted = true;
            }
            // close menu on click outside
            if (_parentMenu == null)
            {
                if (!isMouseOverR())
                { 
                    Close();
                    Fokus = null;
                }
            }
        }
        public void MouseUpHandler()
        {
            if (isMouseOver())
            {
                _mouseRequest = State.Hover;
            }
            else
            {
                _mouseRequest = State.Idle;
                _clickStarted = false;
            }
        }

        // navigation handler
        public void NavigationHandler(Keys key)
        {
            if (Fokus != this) return;

            if (key == Keys.Down)
            {
                if (_parentMenu == null) return;
                int i = 0;
                do
                {
                    i++;
                    int nextIndex = (_menuIndex + i) % _parentMenu._subMenu.Count;
                    Fokus = _parentMenu._subMenu.Values.ToArray<MenuButton>()[nextIndex];
                } while (!Fokus.Enabled);
            }
            if (key == Keys.Up)
            {
                if (_parentMenu == null) return;
                int i = 0;
                do
                {
                    i++;
                    int nextIndex = (_parentMenu._subMenu.Count + _menuIndex - i) % _parentMenu._subMenu.Count;
                    Fokus = _parentMenu._subMenu.Values.ToArray<MenuButton>()[nextIndex];
                } while (!Fokus.Enabled);
            }
            if (key == Keys.Right)
            {
                if (_subMenu.Count == 0) return;
                int i = 0;
                do
                {
                    i++;
                    i %= _subMenu.Count;
                    Fokus = _subMenu.Values.ToArray<MenuButton>()[0];
                } while (!Fokus.Enabled);
            }
            if (key == Keys.Left)
            {
                if (_parentMenu == null) return;
                Fokus = _parentMenu;
            }

            if (key == Keys.Enter)
            {
                Fokus.OnClick?.Invoke();
            }
        }

        // private helper methods
        private void applyDefaultColors()
        {
            BackColor[State.Idle] = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            BackColor[State.Hover] = new Color(0.2f, 0.2f, 0.2f, 1f);
            BackColor[State.Press] = Color.DarkViolet;
            BackColor[State.Disabled] = BackColor[State.Idle];

            ForeColor[State.Idle] = Color.White;
            ForeColor[State.Hover] = Color.White;
            ForeColor[State.Press] = Color.White;
            ForeColor[State.Disabled] = Color.Gray;
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
        private void resolveStateRequests()
        {
            // Disabled
            if (!Enabled)
            {
                State = State.Disabled;
                return;
            }
            // Press
            if (_gamepadRequest == State.Press ||
                _keyboardRequest == State.Press ||
                _mouseRequest == State.Press)
            {
                State = State.Press;
                return;
            }
            // Hover
            if (_gamepadRequest == State.Hover ||
                _keyboardRequest == State.Hover ||
                _mouseRequest == State.Hover ||
                Fokus == this)
            {
                State = State.Hover;
                return;
            }
            // else: Idle
            State = State.Idle;
        }
        #endregion
    }
}
