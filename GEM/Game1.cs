using GEM.Emulation;
using GEM.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;

namespace GEM
{
    /// <summary>
    /// Top Level: MonoGame framework and settings.
    /// </summary>
    public class Game1 : Game
    {

        #region Fields

        static public GraphicsDeviceManager _Graphics;
        static public Game1 _Instance; 
        Emulator _emulator;
        float _fpsSecondCounter;
        int _fpsFrameCounter;
        int _fps;

        static public ContentManager _Content;

        #endregion

        #region Constructors

        public Game1()
        {
            _Graphics = new GraphicsDeviceManager(this);
            _Instance = this;
            Content.RootDirectory = "Content";
            _Content = Content;
            IsMouseVisible = true; 
            Window.AllowUserResizing = false;
            IsFixedTimeStep = true;
            TargetElapsedTime = System.TimeSpan.FromSeconds(70224d / 4194304); // set MonoGame frame rate to original DMG
        }

        #endregion

        #region Methods

        protected override void Initialize()
        {
            _emulator = new Emulator(GraphicsDevice);
            _Graphics.SynchronizeWithVerticalRetrace = false; // disable VSync for fps improvement
            _Graphics.PreferredBackBufferWidth = 1280;
            _Graphics.PreferredBackBufferHeight = 720;
            _Graphics.ApplyChanges(); 

            //save on quit
            Exiting += _emulator.ShutDown;
            

            base.Initialize();
        }


        protected override void LoadContent()
        {
            _emulator.LoadContent(Content) ;
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return;
            _emulator.Update(GraphicsDevice.Viewport);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _emulator.Draw(GraphicsDevice.Viewport);
            
            // FPS Counter
            _fpsSecondCounter += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _fpsFrameCounter++;
            if (_fpsSecondCounter >= 1)
            {
                _fps = _fpsFrameCounter;
                _fpsFrameCounter = 0;
                _fpsSecondCounter = 0;
            }
            Window.Title = string.Format("GEM - {0} - FPS: {1} ({2:0.00} ms)", _emulator.CartridgeTitle, _fps, gameTime.ElapsedGameTime.TotalMilliseconds); // not efficient!
        }

        #endregion

    }
}
