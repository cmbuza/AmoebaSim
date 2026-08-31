using System;
using System.Collections.Generic;
using System.Text;
using AmoebaSim.Core.Primitives;

namespace AmoebaSim.Core.Organisms
{
    public interface ISpatialEntity
    {
        EntityId Id { get; }

        Point Position { get; }

        int Radius { get; }

        SpatialEntityKind Kind { get; }
    }

    public readonly record struct EntityId(long Value);

    public enum SpatialEntityKind
    {
        Amoeba,
        Plant
    }
}
