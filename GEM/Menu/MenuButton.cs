﻿using GEM.Emulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
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
        // menu button events
        public event EventHandler OnPress;
        public event EventHandler OnRelease;
        public event EventHandler OnClick;
        public event EventHandler OnFocus;
        public event EventHandler OnFocusOut;
        // menu structure events
        public event EventHandler OnOpen;
        public event EventHandler OnClose;
        // focus events
        public static event EventHandler OnFocusChange;
        // menu button colors
        public Dictionary<State, Color> BackColor = new Dictionary<State, Color>();
        public Dictionary<State, Color> ForeColor = new Dictionary<State, Color>();
        public Dictionary<State, Color> BorderColor = new Dictionary<State, Color>();
        // menu structure
        public Dictionary<string, MenuButton> SubMenu = new Dictionary<string, MenuButton>();
        MenuButton _parentMenu;
        int _menuIndex;
        // property fields
        State _state = State.Idle;
        Keys _keyBinding;
        Buttons _buttonBinding;
        string _toolTip;
        static MenuButton _focus;
        // button functionality
        bool _clickStarted = false;
        State _gamepadRequest;
        State _keyboardRequest;
        State _mouseRequest;
        State _touchRequest;
        public int ButtonData;
        Image _arrow;
        MenuType _menuType;
        public bool IsClosedOnClickOutside = true;
        #endregion

        #region Constructors
        public MenuButton(BaseControl parentControl = null, MenuButton parentMenu = null, string caption = "", MenuType menuType = MenuType.StandAlone, string image = null, int imagesPerRow = 1) : base(parentControl)
        {
            _parentMenu = parentMenu;
            Label = AddLabel(caption);
            Label.HorizontalAlign = Align.Left;
            if (image != null)
            {
                Image = AddImage(image, imagesPerRow);
                Image.HorizontalAlign = Align.Center;
                Image.VerticalAlign = Align.Center;
                Label.Caption = "";
            }
            _arrow = AddImage("arrow");
            _arrow.HorizontalAlign = Align.Right;
            _arrow.VerticalAlign = Align.Center;
            _arrow.Visible = false;
            PanelAnchorPoint = Add(new BaseControl(this));
            Panel = PanelAnchorPoint.AddPanel();
            Panel.Visible = false;
            _menuType = menuType;
            switch (menuType)
            {
                case MenuType.Click:
                    // submenu open on click
                    OnClick += ToggleMenu;
                    OnFocus += OpenIfSideOpen;
                    break;
                case MenuType.Hover:
                    // submenu open on hover
                    OnFocus += Open;
                    OnClick += ToggleMenu;
                    break;
                default:
                    break;
            }
            OnFocus += CloseSideMenus;
            OnFocus += CloseSubSubmenus;
            OnFocusOut += (o, e) => { _clickStarted = false; _keyboardRequest = State.Idle; _gamepadRequest = State.Idle; };

            // bind mouse events
            Input.OnMouseDown += MouseDownHandler;
            Input.OnMouseUp += MouseUpHandler;

            // default values
            ApplyDefaultColors();
            Width = DEFAULT_WIDTH;
            Height = DEFAULT_HEIGHT;
            PanelAnchorPoint.HorizontalAlign = Align.Right;
            PanelAnchorPoint.VerticalAlign = Align.Top;
            Label.Margin = 15;
        }
        #endregion

        #region Properties
        public Label Label { get; set; }
        public Image Image { get; set; }
        public BaseControl PanelAnchorPoint { get; set; }
        public Panel Panel { get; set; }
        public MenuButton this[string name]
        {
            // submenu access by name
            get
            {
                if (SubMenu.ContainsKey(name))
                {
                    return SubMenu[name];
                } 
                return null;
            }
            set { SubMenu[name] = value; }
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
                    OnFocus?.Invoke(this, EventArgs.Empty);
                    if (_menuType != MenuType.StandAlone) Focus = this;
                }
                // press
                if (value == State.Press && _state != State.Press)
                {
                    _clickStarted = true;
                    OnPress?.Invoke(this, EventArgs.Empty);
                }
                // release
                if (value != State.Press && _state == State.Press)
                {
                    OnRelease?.Invoke(this, EventArgs.Empty);
                }
                // click
                if (value != State.Press && _state == State.Press && _clickStarted)
                {
                    OnClick?.Invoke(this, EventArgs.Empty);
                    _clickStarted = false;
                }
                // hover out
                if (value == State.Idle && _state != State.Idle)
                {
                    OnFocusOut?.Invoke(this, EventArgs.Empty);
                }
                // set value
                _state = value;
                // update button label (color)
                if (Label != null) Label.ForeColor = ForeColor[value];
                // update button image (color and index)
                if (Image != null)
                {
                    Image.ForeColor = ForeColor[value];
                    Image.ImageIndex = (int)value; // cast enum to int
                }
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
        public static MenuButton Focus
        {
            get
            {
                return _focus;
            }
            set
            {
                MenuButton oldFocus = _focus;
                MenuButton newFocus = value;

                // last focus
                if (_focus != null)
                {
                    if (value != _focus)
                    {
                        _focus.OnFocusOut?.Invoke(_focus, EventArgs.Empty);
                    }
                    Input.OnKeyDown -= _focus.NavigationHandlerDown;
                    Input.OnKeyUp -= _focus.NavigationHandlerUp;
                    Input.OnButtonDown -= _focus.NavigationHandlerDown;
                    Input.OnButtonUp -= _focus.NavigationHandlerUp;
                }
                // new focus
                if (value == null)
                {
                    _focus = null;

                    OnFocusChange?.Invoke(null, EventArgs.Empty);
                    return;
                }
                if (value.ParentControl != null)
                {
                    _focus = value;
                }
                if (_focus != null && value.Visible)
                {
                    Input.OnKeyDown += _focus.NavigationHandlerDown;
                    Input.OnKeyUp += _focus.NavigationHandlerUp;
                    Input.OnButtonDown += _focus.NavigationHandlerDown;
                    Input.OnButtonUp += _focus.NavigationHandlerUp;
                }
                
                OnFocusChange?.Invoke(null, EventArgs.Empty);
            }
        }
        public static bool IsFocusSet
        {
            get
            {
                return Focus != null;
            }
        }
        public string ToolTip 
        { 
            get
            {
                return _toolTip != null ? _toolTip : Label.Caption;
            }
            set
            {
                _toolTip = value;
            }
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            if (Visible)
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

                // touch input
                foreach (TouchLocation touch in Input.TouchCollection)
                {
                    if (touch.State == TouchLocationState.Pressed)
                    {
                        if (isTouchOver(touch))
                        {
                            _touchRequest = State.Press;
                        }
                        // close menu on click outside
                        if (_parentMenu == null && IsClosedOnClickOutside) // only check on root button
                        {
                            if (!isTouchOverR(touch))
                            {
                                Close(this, EventArgs.Empty);
                                Focus = null;
                            }
                        }
                    }
                    else
                    {
                        _touchRequest = State.Idle;
                    }
                }
            }

            resolveStateRequests();

            base.Update(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            // highlight button when submenu is visible
            Color color = BackColor[State];
            if (Panel.Visible && SubMenu.Count > 0 && State == State.Idle)
            {
                color = BackColor[State.Hover];
                Label.ForeColor = ForeColor[State.Hover];
                if (Image != null) Image.ForeColor = ForeColor[State.Hover];
            }
            // draw box
            spriteBatch.Draw(_pixel, new Rectangle(LocationX, LocationY, Width, Height), color);
            // draw border
            int borderWidth = 1;
            Color borderColor = BorderColor[State.Idle];
            int x = LocationX - borderWidth / 2;
            int y = LocationY - borderWidth / 2;
            int w = Width + borderWidth;
            int h = Height + borderWidth;
            spriteBatch.Draw(_pixel, new Rectangle(x, y, w, borderWidth), borderColor);
            spriteBatch.Draw(_pixel, new Rectangle(x, y + Height, w, borderWidth), borderColor);
            spriteBatch.Draw(_pixel, new Rectangle(x, y, borderWidth, h), borderColor);
            spriteBatch.Draw(_pixel, new Rectangle(x + Width, y, borderWidth, h), borderColor);
            // draw label, image and submenu
            base.Draw(spriteBatch);
        }

        // add submenu
        public MenuButton AddSubMenu(string name, MenuButton button)
        {
            // check for duplicate name
            if (SubMenu.ContainsKey(name)) return null;

            // add to embedded controls
            Panel.Add(button);

            // add to menu structure
            button._menuIndex = SubMenu.Count; // index for menu navigation
            SubMenu.Add(name, button);
            if (_parentMenu != null) _arrow.Visible = true;

            return button;
        }
        public MenuButton AddSubMenu(string name, string image = null, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT, int imagesPerRow = 1)
        {
            MenuButton tmp = AddSubMenu(name, new MenuButton(Panel, this, name, MenuType.Click, image, imagesPerRow) { Width = width, Height = height });
            if (tmp.Image != null) tmp.Image.ResizeToParent();
            return tmp;
        }

        // menu event handler
        public void ToggleMenu(object sender, EventArgs e)
        {
            if (Panel.Visible)
            {
                Close(this, EventArgs.Empty);
            }
            else
            {
                Open(this, EventArgs.Empty);
            }
        }
        public void Close(object sender, EventArgs e)
        {
            Panel.Visible = false;
            foreach (MenuButton button in SubMenu.Values)
            {
                // close all submenus
                button.Close(this, EventArgs.Empty);
            }
            OnClose?.Invoke(this, EventArgs.Empty);
        }
        public void Open(object sender, EventArgs e)
        {
            if (!Visible) return;
            Focus = this;
            Panel.Visible = true;
            OnOpen?.Invoke(this, EventArgs.Empty);
        }
        public void OpenIfSideOpen(object sender, EventArgs e)
        {
            // used for open menu on hover when another menu on same level is already opened

            if (_parentMenu == null) return;

            bool open = false;
            foreach (MenuButton button in _parentMenu.SubMenu.Values)
            {
                open |= button.Panel.Visible;
            }
            if (open)
            {
                Open(this, EventArgs.Empty);
            }
        }
        public void CloseSideMenus(object sender, EventArgs e)
        {
            // used for closing other menus on same level when menu opened

            if (_parentMenu == null) return;

            foreach (MenuButton button in _parentMenu.SubMenu.Values)
            {
                if (button != this)
                {
                    button.Close(this, EventArgs.Empty);
                }
            }
        }
        public void CloseSubSubmenus(object sender, EventArgs e)
        {
            foreach (MenuButton button in SubMenu.Values)
            {
                button.Close(this, EventArgs.Empty);
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
                resolveStateRequests();
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
            }
        }

        // mouse event handler
        public void MouseDownHandler()
        {
            if (!Visible) return;
            if (isMouseOver())
            {
                _mouseRequest = State.Press;
            }
            // close menu on click outside
            if (_parentMenu == null && IsClosedOnClickOutside) // only check on root button
            {
                if (!isMouseOverR())
                { 
                    Close(this, EventArgs.Empty);
                    Focus = null;
                }
            }
        }
        public void MouseUpHandler()
        {
            if (!Visible) return;
            if (isMouseOver())
            {
                _mouseRequest = State.Hover;
            }
            else
            {
                _mouseRequest = State.Idle;
            }
        }

        // navigation handler
        public void NavigationHandlerDown(Keys key)
        {
            // only continue if fokus on current control
            if (Focus != this) return;

            if (key == Keys.Down)
            {
                navigationRoll(1);
            }
            if (key == Keys.Up)
            {
                navigationRoll(-1);
            }
            if (key == Keys.Right)
            {
                navigationRight();
            }
            if (key == Keys.Left || key == Keys.Escape)
            {
                navigationLeft();
            }

            if (key == Keys.Enter || key == Keys.X)
            {
                if (Focus.Enabled && Focus.Visible)
                {
                    Focus._keyboardRequest = State.Press;
                }
            }
        }
        public void NavigationHandlerDown(Buttons btn)
        {
            // only continue if fokus on current control
            if (Focus != this) return;

            if (btn == Buttons.DPadDown)
            {
                navigationRoll(1);
            }
            if (btn == Buttons.DPadUp)
            {
                navigationRoll(-1);
            }
            if (btn == Buttons.DPadRight)
            {
                navigationRight();
            }
            if (btn == Buttons.DPadLeft || btn == Buttons.B)
            {
                navigationLeft();
            }

            if (btn == Buttons.A || btn == Buttons.Start)
            {
                if (Focus.Enabled && Focus.Visible)
                {
                    Focus._gamepadRequest = State.Press;
                }
            }
        }
        public void NavigationHandlerUp(Keys key)
        {
            if (key == Keys.Enter || key == Keys.X)
            {
                Focus._keyboardRequest = State.Idle;
            }
        }
        public void NavigationHandlerUp(Buttons btn)
        {
            if (btn == Buttons.A || btn == Buttons.Start)
            {
                Focus._gamepadRequest = State.Idle;
            }
        }

        // private helper methods
        public void ApplyDefaultColors()
        {
            BackColor[State.Idle] = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            BackColor[State.Hover] = new Color(0.2f, 0.2f, 0.2f, 1f);
            BackColor[State.Press] = Color.DarkViolet;
            BackColor[State.Disabled] = BackColor[State.Idle];

            ForeColor[State.Idle] = Color.White;
            ForeColor[State.Hover] = Color.White;
            ForeColor[State.Press] = Color.White;
            ForeColor[State.Disabled] = Color.Gray;

            BorderColor[State.Idle] = Color.Transparent;
            BorderColor[State.Hover] = Color.Transparent;
            BorderColor[State.Press] = Color.Transparent;
            BorderColor[State.Disabled] = Color.Transparent;
        }
        private bool isClickStartedR()
        {
            bool started = _clickStarted;
            foreach (MenuButton button in SubMenu.Values)
            {
                started |= button.isClickStartedR();
            }
            return started;
        }
        private bool isMouseOver()
        {
            if (!Input.IsMouseVisible) return false;
            int x = Input.MousePosX;
            int y = Input.MousePosY;
            bool hover = false;
            if (x > LocationX && x < (LocationX + Width) && y > LocationY && y < (LocationY + Height))
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
                foreach (MenuButton sub in SubMenu.Values)
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
                _mouseRequest == State.Press ||
                _touchRequest == State.Press)
            {
                State = State.Press;
                return;
            }
            // Hover
            if (_gamepadRequest == State.Hover ||
                _keyboardRequest == State.Hover ||
                _mouseRequest == State.Hover ||
                Focus == this)
            {
                State = State.Hover;
                return;
            }
            // else: Idle
            State = State.Idle;
        }
        public void SetButtonColors(Color? background, Color? foreground)
        {
            if (background != null)
            {
                BackColor[State.Idle] = (Color)background;
                BackColor[State.Hover] = (Color)background;
                BackColor[State.Press] = (Color)background;
                BackColor[State.Disabled] = (Color)background;
            }
            if (foreground != null)
            {
                ForeColor[State.Idle] = (Color)foreground;
                ForeColor[State.Hover] = (Color)foreground;
                ForeColor[State.Press] = (Color)foreground;
                ForeColor[State.Disabled] = (Color)foreground;
            }
        }
        public void SetStateColorsR(State state, Color background, Color foreground)
        {
            BackColor[state] = background;
            ForeColor[state] = foreground;
            foreach (MenuButton sub in SubMenu.Values)
            {
                sub.SetStateColorsR(state, background, foreground);
            }
        }
        private void navigationRoll(int step = 1)
        {
            if (_parentMenu == null) return;
            int i = 0;
            do
            {
                i++;
                int nextIndex = (_parentMenu.SubMenu.Count + _menuIndex + step * i) % _parentMenu.SubMenu.Count;
                Focus = _parentMenu.SubMenu.Values.ToArray<MenuButton>()[nextIndex];
            } while (!Focus.Enabled || !Focus.Visible); // skip to next entry if control is disabled
        }
        private void navigationRight()
        {
            if (SubMenu.Count == 0) return;
            if (!Panel.Visible) Open(null, EventArgs.Empty);
            int i = 0;
            do
            {
                Focus = SubMenu.Values.ToArray<MenuButton>()[i];
                i++;
                i %= SubMenu.Count;
            } while (!Focus.Enabled || !Focus.Visible);
        }
        private void navigationLeft()
        {
            MenuButton closeMenu;
            if (_parentMenu != null)
            {
                Focus = _parentMenu;
                closeMenu = _parentMenu;
            }
            else
            {
                Focus = null;
                closeMenu = this;
            }
            if (closeMenu.IsClosedOnClickOutside) // used for not closing audioBar
            {
                closeMenu.Close(null, EventArgs.Empty);
            }
            else
            {
                Focus = null;
            }
        }
        private bool isTouchOver(TouchLocation touch)
        {
            int x = (int)touch.Position.X;
            int y = (int)touch.Position.Y;
            bool hover = false;
            if (x > LocationX && x < (LocationX + Width) && y > LocationY && y < (LocationY + Height))
            {
                hover = true;
            }
            return hover;
        }
        private bool isTouchOverR(TouchLocation touch)
        {
            // recursive hover check
            bool hover = isTouchOver(touch);
            if (Panel.Visible)
            {
                foreach (MenuButton sub in SubMenu.Values)
                {
                    hover |= sub.isTouchOverR(touch);
                }
            }
            return hover;
        }
        #endregion
    }
}
