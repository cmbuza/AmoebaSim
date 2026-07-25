using System;
using System.Collections.Generic;
using System.Text;

namespace AmoebaSim.Core.Simulation
{
    public class AmoebaSimContext
    {
        private readonly Random rand;
        public Random Random { get { return rand; } }

        private int worldWidth, worldHeight;
        public int WorldWidth { get { return worldWidth; } internal set { worldWidth = value; } }
        public int WorldHeight { get { return worldHeight; } internal set { worldHeight = value; } }

        public AmoebaSimContext()
        {
            rand = Random.Shared;
            // rand = new Random((int)DateTime.Now.Ticks);
            // rand = new Random(0);
            worldWidth = 1200;
            worldHeight = 900;
        }
    }
}
