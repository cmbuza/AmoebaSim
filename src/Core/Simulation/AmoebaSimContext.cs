using AmoebaSim.Core.Organisms;
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

        private readonly EntityIdGenerator _ids = new();

        internal EntityId NextEntityId()
        {
            return _ids.Next();
        }

        internal AmoebaSimConfig Config { get; }

        internal Random Random { get; }

        internal int WorldWidth => Config.WorldWidth;

        internal int WorldHeight => Config.WorldHeight;
    }
}
