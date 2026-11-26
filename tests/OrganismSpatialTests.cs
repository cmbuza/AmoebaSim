using AmoebaSim.Core.Organisms;
using AmoebaSim.Core.Primitives;
using AmoebaSim.Core.Spatial;

namespace AmoebaSim.Tests;

public class OrganismSpatialTests
{
    [Fact]
    public void MixedIndexFindsEachKindAtItsPhysicalEdge()
    {
        var amoeba = new Amoeba(new EntityId(1), -3, 8, 15);
        var plant = new Plant(new EntityId(2), 18, 8, 6);
        var index = new SpatialHash<ISpatialEntity>(10);
        index.Add(amoeba);
        index.Add(plant);

        // The right edge of the amoeba and the left edge of the plant meet at x = 12.
        var contact = new SpatialBounds(12, 8, 12, 8);
        var results = new List<ISpatialEntity>();
        index.Query(contact, SpatialEntityKind.Amoeba, results);
        Assert.Same(amoeba, Assert.Single(results));

        index.Query(contact, SpatialEntityKind.Plant, results);
        Assert.Same(plant, Assert.Single(results));
    }

    [Fact]
    public void BoundsReflectTheCurrentAmoebaSizeAndPlantLocationAndRadius()
    {
        var amoeba = new Amoeba(new EntityId(1), -3, 8, 15);
        var plant = new Plant(new EntityId(2), 18, 8, 6);
        Assert.Equal(new SpatialBounds(-18, -7, 12, 23), amoeba.Bounds);
        Assert.Equal(new SpatialBounds(12, 2, 24, 14), plant.Bounds);

        amoeba.R = 20;
        plant.Location = new Point(5, -17);
        plant.Radius = 4;

        Assert.Equal(new Point(-3, 8), amoeba.Position);
        Assert.Equal(new SpatialBounds(-23, -12, 17, 28), amoeba.Bounds);
        Assert.Equal(new Point(5, -17), plant.Position);
        Assert.Equal(new SpatialBounds(1, -21, 9, -13), plant.Bounds);
    }
}
