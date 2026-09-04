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

    public readonly record struct EntityId(long Value)
    {
        public static EntityId None => default;

        public bool IsAssigned => Value > 0;

        public override string ToString() => Value.ToString();
    }
    internal sealed class EntityIdGenerator
    {
        private long _lastIssued;

        public EntityId Next()
        {
            return new EntityId(checked(++_lastIssued));
        }

        public void AdvancePast(EntityId existingId)
        {
            if (existingId.Value > _lastIssued)
                _lastIssued = existingId.Value;
        }
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
