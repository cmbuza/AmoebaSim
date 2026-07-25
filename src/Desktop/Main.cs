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

        private List<Amoeba> organisms;
        private List<Core.Organisms.Plant> plants;
        private bool drawViewField = true;
        private bool drawSmellField = true;

        private const double PlantGrowIntervalSeconds = 60.0;
        private double _plantGrowAccumulator = 0.0;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = BACKBUFFER_WIDTH;
            graphics.PreferredBackBufferHeight = BACKBUFFER_HEIGHT;
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(200000);

            organisms = new List<Amoeba>();
            plants = new List<Plant>();

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

            for (int i = 0; i < NUM_INITIAL_ORGANISMS; i++)
            {
                organisms.Add(
                    new Amoeba(
                        Random.Shared.Next(gfxConfig.BackBufferWidth),
                        Random.Shared.Next(gfxConfig.BackBufferHeight),
                        Random.Shared.Next(10, 50),
                        1,
                        Random.Shared.Next(20, 100),
                        Random.Shared.Next(1, 5),
                        Random.Shared.Next(2, 7),
                        Random.Shared.Next(30, 100)));
                //organisms.Add(
                //        new Organism(
                //            Rand.Next(gfxConfig.BackBufferWidth),
                //            Rand.Next(gfxConfig.BackBufferHeight),
                //            20,
                //            1,
                //            20,
                //            1,
                //            3));
            }

            Grow(NUM_PLANTS_PER_GROW, context);

            Console.WriteLine("New Sim began at " + DateTime.Now.TimeOfDay);
        }

        unsafe void ClearBackBuffer()
        {
            fixed (Color* cp = backBuffer)
            {
                for (int x = 0; x < backBuffer.Length; x++)
                    cp[x] = Color.White;
            }
        }

        private AmoebaSimContext context = new AmoebaSimContext();

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

            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Escape))
                Exit();

            if (organisms.Count < 1 || Keyboard.GetState().GetPressedKeys().Contains(Keys.N))
            {
                organisms.Clear();
                Console.WriteLine("All organisms died out at " + DateTime.Now.TimeOfDay);
                LoadContent();
                return;
            }

            int iwp = 0, iwo = 0;

            List<Amoeba> newOrganisms = new List<Amoeba>();

            foreach (Amoeba o in organisms)
            {
                try
                {

                    if (o.IsAlive)
                    {
                        o.Update(gameTime.ElapsedGameTime.TotalSeconds, context);

                        if (o.IsInHeat)
                        {
                            foreach (Amoeba org in organisms)
                            {
                                if (!org.IsAlive) continue;
                                if (org == o) continue;
                                if (!org.IsInHeat) continue;

                                iwo = o.InteractsWithOrganism(org);
                                if (iwo == 0) continue;

                                if (iwo == 1)
                                    o.Target = new Core.Primitives.Point(org.X, org.Y);
                                else
                                {
                                    o.PlantsEaten = 0;
                                    org.PlantsEaten = 0;
                                    newOrganisms.AddRange(o.Reproduce(org));
                                }
                            }
                        }
                        else
                        {
                            foreach (Plant p in plants)
                            {
                                if (p.Eaten) continue;

                                iwp = o.InteractsWithPlant(p);
                                if (iwp == 0) continue;

                                if (iwp == 1)
                                    o.Target = p.Location;
                                else
                                {
                                    p.Eaten = true;
                                    o.PlantsEaten++;
                                }
                            }
                        }
                    }
                }
                catch (InvalidOperationException ioe)
                {
                    System.Diagnostics.Debug.WriteLine(ioe);
                }
            }

            organisms.RemoveAll(Amoeba.IsNotAlive);
            organisms.AddRange(newOrganisms);
            plants.RemoveAll(Plant.IsEaten);

            _plantGrowAccumulator += gameTime.ElapsedGameTime.TotalSeconds;

            if (_plantGrowAccumulator >= PlantGrowIntervalSeconds)
            {
                Grow(NUM_PLANTS_PER_GROW, context);
                _plantGrowAccumulator = 0.0;
            }


            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {

            ClearBackBuffer();

            try
            {
                foreach (Plant p in plants)
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

            foreach (Amoeba o in organisms)
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

        private void Grow(int numToGrow, AmoebaSimContext context)
        {
            for (int i = 0; i < numToGrow; i++)
                plants.Add( new Plant(
                        context.Random.Next(context.WorldWidth),
                        context.Random.Next(context.WorldHeight),
                        context.Random.Next(5, 30)
                        )
                    );
        }
    }
}
