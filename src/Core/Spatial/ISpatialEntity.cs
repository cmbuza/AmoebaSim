using System;
using System.Collections.Generic;
using System.Text;
using AmoebaSim.Core.Primitives;

namespace AmoebaSim.Core.Spatial

{
    public interface ISpatialEntity
    {
        EntityId Id { get; }

        Point Position { get; }

        SpatialBounds Bounds { get; }

        SpatialEntityKind Kind { get; }
    }

    public readonly record struct SpatialBounds(
        long MinX,
        long MinY,
        long MaxX,
        long MaxY)
    {
        public static SpatialBounds Around(Point center, long radius)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(radius);

            return new SpatialBounds(
                checked((long)center.X - radius),
                checked((long)center.Y - radius),
                checked((long)center.X + radius),
                checked((long)center.Y + radius));
        }

        public bool Intersects(SpatialBounds other) =>
            MinX <= other.MaxX &&
            MaxX >= other.MinX &&
            MinY <= other.MaxY &&
            MaxY >= other.MinY;
    }

    public readonly record struct EntityId(long Value)
    {
        public static EntityId None => default;

        public bool IsAssigned => Value > 0;

        public override string ToString() => Value.ToString();
    }

    [Flags]
    public enum SpatialEntityKind
    {
        None = 0,
        Amoeba = 1 << 0,
        Plant = 1 << 1,
        All = Amoeba | Plant
    }
}
