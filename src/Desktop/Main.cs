using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using AmoebaSim.Core.Primitives;
using AmoebaSim.Core.Organisms;
using AmoebaSim.Core.Simulation;

namespace AmoebaSim.Desktop
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Game
    {
        public static int BACKBUFFER_WIDTH = 1200;
        public static int BACKBUFFER_HEIGHT = 900;

        private static int NUM_INITIAL_ORGANISMS = 3;
        private static int NUM_PLANTS_PER_GROW = 25;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private PresentationParameters gfxConfig;

        private Texture2D dummyTex;

        private Color[] backBuffer;
        private Texture2D backBufferTex;

       /* private List<Amoeba> organisms;
        private List<Plant> plants;*/
        private bool drawViewField = true;
        private bool drawSmellField = true;

       /* private const double PlantGrowIntervalSeconds = 60.0;
        private double _plantGrowAccumulator = 0.0;*/

        AmoebaSimulation simulation;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = BACKBUFFER_WIDTH;
            graphics.PreferredBackBufferHeight = BACKBUFFER_HEIGHT;
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(200000);

            /*organisms = new List<Amoeba>();
            plants = new List<Plant>();*/

            simulation = new AmoebaSimulation(new AmoebaSimConfig());

        }

        protected override void Initialize()
        {
            gfxConfig = GraphicsDevice.PresentationParameters;
            base.Initialize();
        }

        protected override void UnloadContent()
        {
            dummyTex.Dispose();
            backBufferTex.Dispose();

            base.UnloadContent();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            backBuffer = new Color[gfxConfig.BackBufferWidth *
                                   gfxConfig.BackBufferHeight];


            backBufferTex = new Texture2D(GraphicsDevice,
                                          gfxConfig.BackBufferWidth,
                                          gfxConfig.BackBufferHeight);

            dummyTex = new Texture2D(GraphicsDevice, 1, 1);
            dummyTex.SetData(new Color[] { Color.White });
        }

        unsafe void ClearBackBuffer()
        {
            fixed (Color* cp = backBuffer)
            {
                for (int x = 0; x < backBuffer.Length; x++)
                    cp[x] = Color.White;
            }
        }

        double fpsTimer = 0;
        int frames = 0;
        protected override void Update(GameTime gameTime)
        {
            fpsTimer += gameTime.ElapsedGameTime.TotalSeconds;
            frames++;

            if (fpsTimer >= 1.0)
            {
                //System.Diagnostics.Debug.WriteLine("FPS = " + frames);

                frames = 0;
                fpsTimer = 0;
            }

            KeyboardState kbs = Keyboard.GetState();

            if (kbs.GetPressedKeys().Contains(Keys.Escape))
                Exit();

            if (kbs.GetPressedKeys().Contains(Keys.N) || simulation.IsExtinct)
            {
                Console.WriteLine("All organisms died out at " + DateTime.Now.TimeOfDay);
                simulation.Reset();
                Console.WriteLine("New Sim began at " + DateTime.Now.TimeOfDay);
            }

            simulation.Advance(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {

            ClearBackBuffer();

            try
            {
                foreach (Plant p in simulation.Plants)
                {
                    ZiggyVector.DrawCircleFilled(backBuffer,
                        gfxConfig.BackBufferWidth,
                        gfxConfig.BackBufferHeight,
                        p.Location.X,
                        p.Location.Y,
                        p.Radius,
                        Color.Green);
                }
            }
            catch (InvalidOperationException ioe)
            {
                System.Diagnostics.Debug.WriteLine(ioe);
            }

            foreach (Amoeba o in simulation.Amoebas)
            {
                if (!o.IsAlive) continue;

                Color c = Color.Blue;
                if (o.IsInHeat)
                    c = Color.Red;

                ZiggyVector.DrawCircleFilled(backBuffer,
                gfxConfig.BackBufferWidth,
                gfxConfig.BackBufferHeight,
                o.X,
                o.Y,
                o.R,
                c);

                if (drawViewField)
                {
                    ZiggyVector.DrawCircle(backBuffer,
                        gfxConfig.BackBufferWidth,
                        gfxConfig.BackBufferHeight,
                        o.X,
                        o.Y,
                        o.ViewDistance,
                        Color.Violet);
                }

                if (drawSmellField)
                {
                    ZiggyVector.DrawCircle(backBuffer,
                       gfxConfig.BackBufferWidth,
                       gfxConfig.BackBufferHeight,
                       o.X,
                       o.Y,
                       o.SmellDistance,
                       Color.SlateGray);
                }
            }

            backBufferTex.SetData(backBuffer);


            spriteBatch.Begin();
            spriteBatch.Draw(backBufferTex, Vector2.Zero, Color.White);
            spriteBatch.Draw(dummyTex, -Vector2.UnitX, Color.White);
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
