using AmoebaSim.Core.Simulation;

namespace AmoebaSim.Tests;

public class SimulationConfigTests
{
    [Fact]
    public void ConstructorRequiresAConfig()
    {
        Assert.Throws<ArgumentNullException>(() => new AmoebaSimulation(null!));
    }

    [Fact]
    public void WorldDimensionsAndSpatialCellSizeMustBePositive()
    {
        AssertInvalid(new() { WorldWidth = 0 }, nameof(AmoebaSimConfig.WorldWidth));
        AssertInvalid(new() { WorldHeight = 0 }, nameof(AmoebaSimConfig.WorldHeight));
        AssertInvalid(new() { SpatialCellSize = 0 }, nameof(AmoebaSimConfig.SpatialCellSize));
    }

    [Fact]
    public void PopulationCountsCannotBeNegative()
    {
        AssertInvalid(new() { InitialAmoebaCount = -1 }, nameof(AmoebaSimConfig.InitialAmoebaCount));
        AssertInvalid(new() { InitialPlantCount = -1 }, nameof(AmoebaSimConfig.InitialPlantCount));
        AssertInvalid(new() { PlantsPerGrowth = -1 }, nameof(AmoebaSimConfig.PlantsPerGrowth));
    }

    [Fact]
    public void SimulationDurationsMustBePositive()
    {
        AssertInvalid(new() { FixedStep = TimeSpan.Zero }, nameof(AmoebaSimConfig.FixedStep));
        AssertInvalid(new() { AmoebaLifeSpan = TimeSpan.Zero }, nameof(AmoebaSimConfig.AmoebaLifeSpan));
        AssertInvalid(new() { PlantGrowthInterval = TimeSpan.Zero }, nameof(AmoebaSimConfig.PlantGrowthInterval));
    }

    [Fact]
    public void MutationProbabilityDenominatorMustBePositive()
    {
        AssertInvalid(new() { MutationProbability = 0 }, nameof(AmoebaSimConfig.MutationProbability));
    }

    private static void AssertInvalid(AmoebaSimConfig config, string parameter)
    {
        Assert.Throws<ArgumentOutOfRangeException>(parameter, () => new AmoebaSimulation(config));
    }
}
