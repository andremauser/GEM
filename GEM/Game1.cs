using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GEM
{
    /// <summary>
    /// Top Level: MonoGame framework and settings.
    /// </summary>
    public class Game1 : Game
    {

        #region Fields

        GraphicsDeviceManager _graphics;
        Emulator _emulator;
        float _fpsSecondCounter;
        int _fpsFrameCounter;
        int _fps;

        #endregion

        #region Constructors

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true; 
            Window.AllowUserResizing = true;
            IsFixedTimeStep = true;
            TargetElapsedTime = System.TimeSpan.FromSeconds(70224d / 4194304); // set MonoGame frame rate to original DMG
        }

        #endregion

        #region Methods

        protected override void Initialize()
        {
            _emulator = new Emulator(GraphicsDevice);
            _graphics.SynchronizeWithVerticalRetrace = false; // disable VSync for fps improvement
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges(); 

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
