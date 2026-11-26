using AmoebaSim.Core.Organisms;
using AmoebaSim.Core.Primitives;
using AmoebaSim.Core.Simulation;
using AmoebaSim.Core.Spatial;

namespace AmoebaSim.Tests;

public class SimulationTests
{
    [Fact]
    public void ConstructorPopulatesTheConfiguredWorldWithDistinctEntityIds()
    {
        var config = new AmoebaSimConfig
        {
            RandomSeed = 731,
            WorldWidth = 39,
            WorldHeight = 17,
            InitialAmoebaCount = 4,
            InitialPlantCount = 7
        };
        var simulation = new AmoebaSimulation(config);

        Assert.Equal(0, simulation.Tick);
        Assert.False(simulation.IsExtinct);
        Assert.Equal(config.InitialAmoebaCount, simulation.Amoebas.Count);
        Assert.Equal(config.InitialPlantCount, simulation.Plants.Count);
        var ids = new HashSet<EntityId>();
        foreach (var entity in simulation.Amoebas.Cast<ISpatialEntity>().Concat(simulation.Plants))
        {
            Assert.True(entity.Id.IsAssigned);
            Assert.True(ids.Add(entity.Id), $"Duplicate entity ID {entity.Id}");
            Assert.InRange(entity.Position.X, 0, config.WorldWidth - 1);
            Assert.InRange(entity.Position.Y, 0, config.WorldHeight - 1);
        }
    }

    [Fact]
    public void SeededSimulationReplaysAcrossDifferentFrameDurations()
    {
        var config = new AmoebaSimConfig
        {
            RandomSeed = 85,
            FixedStep = TimeSpan.FromMilliseconds(250),
            PlantGrowthInterval = TimeSpan.FromSeconds(2),
            PlantsPerGrowth = 2
        };
        var stepped = new AmoebaSimulation(config);
        var advanced = new AmoebaSimulation(config);
        var amoebaState = (Amoeba a) => new
        {
            a.Id, a.Position, a.Target, a.Direction, a.PlantsEaten, a.R, a.Speed,
            a.ViewDistance, a.SmellDistance, a.NumberOfOffspring, a.FoodNeededToReproduce
        };

        for (int second = 0; second < 16; second++)
        {
            for (int tick = 0; tick < 4; tick++)
                stepped.Step();

            advanced.Advance(TimeSpan.FromMilliseconds(375));
            advanced.Advance(TimeSpan.FromMilliseconds(625));

            Assert.Equal(4L * (second + 1), advanced.Tick);
            Assert.Equal(stepped.Amoebas.Select(amoebaState), advanced.Amoebas.Select(amoebaState));
            Assert.Equal(stepped.Plants.Select(p => (p.Id, p.Location, p.Radius)),
                         advanced.Plants.Select(p => (p.Id, p.Location, p.Radius)));
        }
    }

    [Fact]
    public void AdvanceRejectsNegativeElapsedTimeAndPreservesPendingTime()
    {
        var simulation = new AmoebaSimulation(new AmoebaSimConfig
        {
            RandomSeed = 11,
            FixedStep = TimeSpan.FromSeconds(1)
        });

        simulation.Advance(TimeSpan.FromMilliseconds(750));
        Assert.Throws<ArgumentOutOfRangeException>(() => simulation.Advance(TimeSpan.FromTicks(-1)));
        simulation.Advance(TimeSpan.Zero);
        Assert.Equal(0, simulation.Tick);

        simulation.Advance(TimeSpan.FromMilliseconds(250));
        Assert.Equal(1, simulation.Tick);
    }

    [Fact]
    public void StepRemovesAmoebasAtTheirLifespanAndGrowsPlantsAtTheInterval()
    {
        var config = new AmoebaSimConfig
        {
            RandomSeed = 21,
            InitialAmoebaCount = 1,
            InitialPlantCount = 0,
            FixedStep = TimeSpan.FromSeconds(1),
            AmoebaLifeSpan = TimeSpan.FromSeconds(2),
            PlantGrowthInterval = TimeSpan.FromSeconds(2),
            PlantsPerGrowth = 3
        };
        var simulation = new AmoebaSimulation(config);

        simulation.Step();
        Assert.False(simulation.IsExtinct);
        Assert.Empty(simulation.Plants);

        simulation.Step();
        Assert.True(simulation.IsExtinct);
        Assert.Empty(simulation.Amoebas);
        Assert.Equal(3, simulation.Plants.Count);

        simulation.Step();
        Assert.Equal(3, simulation.Plants.Count);
        simulation.Step();
        Assert.Equal(6, simulation.Plants.Count);
        Assert.Equal(6, simulation.Plants.Select(p => p.Id).Distinct().Count());
    }

    [Fact]
    public void ResetRepopulatesAnExtinctWorldAndClearsBothPendingTimers()
    {
        var config = new AmoebaSimConfig
        {
            RandomSeed = 32,
            InitialAmoebaCount = 1,
            InitialPlantCount = 0,
            FixedStep = TimeSpan.FromSeconds(1),
            AmoebaLifeSpan = TimeSpan.FromSeconds(1),
            PlantGrowthInterval = TimeSpan.FromSeconds(2),
            PlantsPerGrowth = 1
        };
        var simulation = new AmoebaSimulation(config);

        simulation.Advance(TimeSpan.FromMilliseconds(3500));
        Assert.True(simulation.IsExtinct);
        Assert.Single(simulation.Plants);

        simulation.Reset();
        Assert.Equal(0, simulation.Tick);
        Assert.Single(simulation.Amoebas);
        Assert.False(simulation.IsExtinct);
        Assert.Empty(simulation.Plants);

        simulation.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(0, simulation.Tick);
        simulation.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(1, simulation.Tick);
        Assert.Empty(simulation.Plants);
    }

    [Fact]
    public void TwoAmoebasInReachOfOnePlantConsumeItOnlyOnce()
    {
        var simulation = CreateWorldWithTwoNeighbors(plantCount: 1);

        simulation.Step();

        Assert.Empty(simulation.Plants);
        Assert.Equal(1, simulation.Amoebas.Sum(a => a.PlantsEaten));
    }

    [Fact]
    public void StepConsumesAPlantAtExactCircularContact()
    {
        // A radius-10 amoeba and radius-5 plant touch at distance 15.
        var simulation = CreateWorldWithPlantAtOffset(9, 12);

        simulation.Step();

        Assert.Equal(1, simulation.Amoebas[0].PlantsEaten);
        Assert.Empty(simulation.Plants);
    }

    [Theory]
    [InlineData(11, 11, true)]  // Bounding boxes overlap, but the circles do not.
    [InlineData(30, 0, true)]   // The plant's edge is exactly at sight range.
    [InlineData(30, 1, false)]  // Just outside sight range.
    public void StepTargetsPlantsOutsideContactOnlyWithinSight(int offsetX, int offsetY, bool targeted)
    {
        var simulation = CreateWorldWithPlantAtOffset(offsetX, offsetY);

        simulation.Step();

        var plant = Assert.Single(simulation.Plants);
        var amoeba = simulation.Amoebas[0];
        Assert.Equal(0, amoeba.PlantsEaten);
        Assert.Equal(targeted, amoeba.Target == plant.Location);
    }

    [Fact]
    public void TwoFedNeighborsProduceOneBroodAndResetTheirFoodCounts()
    {
        var simulation = CreateWorldWithTwoNeighbors(plantCount: 0);
        var parents = simulation.Amoebas.ToArray();
        foreach (var parent in parents)
        {
            parent.NumberOfOffspring = 2;
            parent.PlantsEaten = parent.FoodNeededToReproduce;
        }

        simulation.Step();

        Assert.Equal(4, simulation.Amoebas.Count);
        Assert.All(parents, parent =>
        {
            Assert.Contains(parent, simulation.Amoebas);
            Assert.Equal(0, parent.PlantsEaten);
        });
        Assert.All(simulation.Amoebas, a => Assert.True(a.Id.IsAssigned));
        Assert.Equal(4, simulation.Amoebas.Select(a => a.Id).Distinct().Count());
    }

    private static AmoebaSimulation CreateWorldWithPlantAtOffset(int offsetX, int offsetY)
    {
        var simulation = new AmoebaSimulation(new AmoebaSimConfig
        {
            RandomSeed = 97,
            InitialAmoebaCount = 1,
            InitialPlantCount = 1
        });
        var amoeba = simulation.Amoebas[0];
        amoeba.Speed = 0;
        amoeba.R = 10;
        amoeba.ViewDistance = 25;
        amoeba.SmellDistance = 0;
        var plant = simulation.Plants[0];
        plant.Radius = 5;
        // Allow the initial wander target to be chosen with the plant out of reach.
        plant.Location = new Point(amoeba.X + 100, amoeba.Y);
        simulation.Step();
        amoeba.Target = new Point(amoeba.X - 1, amoeba.Y);
        plant.Location = new Point(amoeba.X + offsetX, amoeba.Y + offsetY);
        return simulation;
    }

    private static AmoebaSimulation CreateWorldWithTwoNeighbors(int plantCount)
    {
        var simulation = new AmoebaSimulation(new AmoebaSimConfig
        {
            RandomSeed = 97,
            WorldWidth = 25,
            WorldHeight = 20,
            InitialAmoebaCount = 2,
            InitialPlantCount = plantCount
        });
        // Each organism's reach covers the world, regardless of where it spawns.
        foreach (var amoeba in simulation.Amoebas)
        {
            amoeba.R = 40;
            amoeba.ViewDistance = 100;
        }
        return simulation;
    }
}
