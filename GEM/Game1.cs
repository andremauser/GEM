using GEM.Emulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
        static public ContentManager _Content;
        static public int CPU_FREQ = 4194304;
        static public int FRAME_CYCLES = 70224;
        static public double FRAME_RATE = 1d * CPU_FREQ / FRAME_CYCLES;
        #endregion

        #region Constructors
        public Game1()
        {
            _Graphics = new GraphicsDeviceManager(this);
            _Instance = this;
            Content.RootDirectory = "Content";
            _Content = Content;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;// false;
            IsFixedTimeStep = true;
            TargetElapsedTime = System.TimeSpan.FromSeconds(1d / FRAME_RATE); // set MonoGame frame rate to original DMG
        }
        #endregion

        #region Properties
        public int FPS { get; private set; } 
        #endregion

        #region Methods
        protected override void Initialize()
        {
            _emulator = new Emulator(GraphicsDevice);
            _Graphics.PreferredBackBufferWidth = 800;
            _Graphics.PreferredBackBufferHeight = 720;
            _Graphics.ApplyChanges();
            Window.Title = "GEM";

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
                FPS = _fpsFrameCounter;
                _fpsFrameCounter = 0;
                _fpsSecondCounter = 0;
            }
        }
        #endregion

    }
}
