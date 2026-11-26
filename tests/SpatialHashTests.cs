using AmoebaSim.Core.Primitives;
using AmoebaSim.Core.Spatial;

namespace AmoebaSim.Tests;

public class SpatialHashTests
{
    private sealed record TestEntity(
        EntityId Id, Point Position, SpatialBounds Bounds, SpatialEntityKind Kind) : ISpatialEntity;

    private static TestEntity Entity(long id, int x, int y, int radius = 0,
        SpatialEntityKind kind = SpatialEntityKind.Plant)
    {
        var position = new Point(x, y);
        return new TestEntity(new EntityId(id), position, SpatialBounds.Around(position, radius), kind);
    }

    [Fact]
    public void Query_FindsAnEntityFromTheFarCornerOfItsBounds()
    {
        var index = new SpatialHash<TestEntity>(10);
        var spanning = Entity(1, -10, -10, 20);
        index.Add(spanning);
        index.Add(Entity(2, 11, 11)); // Same cell as the query, outside its bounds.
        List<TestEntity> results = [];

        index.Query(new SpatialBounds(10, 10, 10, 10), SpatialEntityKind.All, results);

        Assert.Same(spanning, Assert.Single(results));
    }

    [Fact]
    public void Query_FindsBoundsThatExtendBeyondIntCoordinates()
    {
        var index = new SpatialHash<TestEntity>(10);
        var entity = Entity(1, int.MaxValue, int.MinValue, 5);
        index.Add(entity);
        var area = new SpatialBounds((long)int.MaxValue + 4, (long)int.MinValue - 4,
                                     (long)int.MaxValue + 5, (long)int.MinValue - 3);
        List<TestEntity> results = [];

        index.Query(area, SpatialEntityKind.All, results);

        Assert.Same(entity, Assert.Single(results));
    }

    [Fact]
    public void Query_ReplacesResultsAndReturnsEachMatchingIdOnceInAscendingOrder()
    {
        var index = new SpatialHash<TestEntity>(8);
        var amoeba = Entity(3, 1, 1, 9, SpatialEntityKind.Amoeba);
        var plant = Entity(1, 4, 0, 8);
        var otherPlant = Entity(2, 0, 0);
        index.Add(amoeba);
        index.Add(plant);
        index.Add(otherPlant);
        var area = new SpatialBounds(-10, -10, 12, 12);
        List<TestEntity> results = [];

        index.Query(area, SpatialEntityKind.All, results, otherPlant.Id);
        Assert.Equal(new[] { plant, amoeba }, results);

        index.Query(area, SpatialEntityKind.Plant, results);
        Assert.Equal(new[] { plant, otherPlant }, results);

        index.Query(area, SpatialEntityKind.None, results);
        Assert.Empty(results);
    }

    [Fact]
    public void Clear_AllowsRebuildingWithTheSameIdAtANewLocation()
    {
        var index = new SpatialHash<TestEntity>(4);
        var before = Entity(7, 3, 3, 1);
        var after = Entity(7, -30, -30, 1);
        List<TestEntity> results = [];
        index.Add(before);
        index.Query(before.Bounds, SpatialEntityKind.All, results);
        Assert.Same(before, Assert.Single(results));

        index.Clear();
        index.Add(after);

        index.Query(before.Bounds, SpatialEntityKind.All, results);
        Assert.Empty(results);
        index.Query(after.Bounds, SpatialEntityKind.All, results);
        Assert.Same(after, Assert.Single(results));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(32)]
    public void Query_MatchesBruteForceAcrossCellSizes(int cellSize)
    {
        var random = new Random(7523);
        var index = new SpatialHash<TestEntity>(cellSize);
        List<TestEntity> entities = [];
        for (int id = 40; id > 0; id--)
        {
            var kind = id % 2 == 0 ? SpatialEntityKind.Amoeba : SpatialEntityKind.Plant;
            var entity = Entity(id, random.Next(-35, 36), random.Next(-35, 36), random.Next(0, 10), kind);
            entities.Add(entity);
            index.Add(entity);
        }
        var wall = new TestEntity(new EntityId(41), new Point(0, -8),
            new SpatialBounds(-64, -9, 64, -7), SpatialEntityKind.Plant);
        entities.Add(wall);
        index.Add(wall);

        SpatialEntityKind[] masks = [SpatialEntityKind.Amoeba, SpatialEntityKind.Plant, SpatialEntityKind.All];
        List<TestEntity> results = [];
        foreach (var observer in entities)
        {
            var area = SpatialBounds.Around(observer.Position, 12);
            foreach (var mask in masks)
            {
                var expected = entities
                    .Where(e => e.Id != observer.Id && (e.Kind & mask) != 0)
                    .Where(e => e.Bounds.MinX <= area.MaxX && area.MinX <= e.Bounds.MaxX &&
                                e.Bounds.MinY <= area.MaxY && area.MinY <= e.Bounds.MaxY)
                    .Select(e => e.Id.Value)
                    .Order();

                index.Query(area, mask, results, observer.Id);

                Assert.Equal(expected, results.Select(e => e.Id.Value));
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsNonpositiveCellSize(int cellSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpatialHash<TestEntity>(cellSize));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Add_RejectsUnassignedIds(int id)
    {
        var index = new SpatialHash<TestEntity>(8);
        Assert.Throws<ArgumentException>(() => index.Add(Entity(id, 0, 0)));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    public void AddAndQuery_RejectBoundsWithAnInvertedAxis(int minX, int minY)
    {
        var index = new SpatialHash<TestEntity>(8);
        var bounds = new SpatialBounds(minX, minY, 0, 0);
        var entity = Entity(1, 0, 0) with { Bounds = bounds };
        Assert.Throws<ArgumentException>(() => index.Add(entity));
        Assert.Throws<ArgumentException>(() => index.Query(bounds, SpatialEntityKind.All, []));
    }
}
