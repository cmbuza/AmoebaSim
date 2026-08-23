using System;
using System.Collections.Generic;
using System.Text;

namespace AmoebaSim.Core.Simulation
{
    internal sealed class AmoebaSimContext
    {
        internal AmoebaSimContext(AmoebaSimConfig config)
        {
            Config = config;

            Random = config.RandomSeed is int seed
                ? new Random(seed)
                : new Random();
        }

        internal AmoebaSimConfig Config { get; }

        internal Random Random { get; }

        internal int WorldWidth => Config.WorldWidth;

        internal int WorldHeight => Config.WorldHeight;
    }
}
