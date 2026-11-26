using AmoebaSim.Core.Primitives;
using AmoebaSim.Core.Spatial;

namespace AmoebaSim.Tests;

public class SpatialBoundsTests
{
    [Fact]
    public void Around_ZeroRadiusProducesAPoint()
    {
        Assert.Equal(new SpatialBounds(-3, 7, -3, 7), SpatialBounds.Around(new Point(-3, 7), 0));
    }

    [Fact]
    public void Around_ExtendsPastBothIntegerCoordinateLimits()
    {
        var center = new Point(int.MinValue, int.MaxValue);
        var expected = new SpatialBounds(-2_147_483_651, 2_147_483_644, -2_147_483_645, 2_147_483_650);

        Assert.Equal(expected, SpatialBounds.Around(center, 3));
    }

    [Fact]
    public void Around_RejectsNegativeRadius()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SpatialBounds.Around(default, -1));
    }

    [Fact]
    public void Around_ThrowsWhenBoundsCannotBeRepresented()
    {
        Assert.Throws<OverflowException>(() => SpatialBounds.Around(new Point(1, 0), long.MaxValue));
    }

    [Theory]
    [InlineData(-2, -2, 2, 2, true)]   // contained
    [InlineData(3, 3, 12, 5, true)]    // partial overlap
    [InlineData(5, -2, 9, 2, true)]    // shared edge
    [InlineData(5, 5, 5, 5, true)]     // point on a corner
    [InlineData(6, -2, 9, 2, false)]   // gap along X
    [InlineData(-2, -9, 2, -6, false)] // gap along Y
    public void Intersects_UsesInclusiveBoundsInBothDirections(
        int minX, int minY, int maxX, int maxY, bool intersects)
    {
        var area = new SpatialBounds(-5, -5, 5, 5);
        var other = new SpatialBounds(minX, minY, maxX, maxY);

        Assert.Equal(intersects, area.Intersects(other));
        Assert.Equal(intersects, other.Intersects(area));
    }
}
