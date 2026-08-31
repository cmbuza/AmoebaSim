using System;
using System.Collections.Generic;
using System.Text;

namespace AmoebaSim.Core.Simulation
{
    public sealed record AmoebaSimConfig
    {
        public int WorldWidth { get; init; } = 1200;
        public int WorldHeight { get; init; } = 900;

        public int InitialAmoebaCount { get; init; } = 3;
        public int InitialPlantCount { get; init; } = 25;

        public TimeSpan AmoebaLifeSpan { get; init; }
            = TimeSpan.FromSeconds(60);

        public int PlantsPerGrowth { get; init; } = 25;
        public TimeSpan PlantGrowthInterval { get; init; }
            = TimeSpan.FromSeconds(60);

        // Preserves the legacy 20 ms simulation step.
        public TimeSpan FixedStep { get; init; }
            = TimeSpan.FromMilliseconds(20);

        // Null means generate a nondeterministic seed.
        public int? RandomSeed { get; init; }

        //read chances as 1 in MutationProbability
        public int MutationProbability { get; init; } = 10; 

        internal void Validate()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(WorldWidth);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(WorldHeight);
            ArgumentOutOfRangeException.ThrowIfNegative(InitialAmoebaCount);
            ArgumentOutOfRangeException.ThrowIfNegative(InitialPlantCount);
            ArgumentOutOfRangeException.ThrowIfNegative(PlantsPerGrowth);

            if (PlantGrowthInterval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(PlantGrowthInterval));

            if (FixedStep <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(FixedStep));

            if (AmoebaLifeSpan <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(AmoebaLifeSpan));
        }
    }
}
