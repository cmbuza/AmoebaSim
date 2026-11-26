using AmoebaSim.Core.Spatial;

namespace AmoebaSim.Core.Simulation
{
    internal sealed class EntityIdGenerator
    {
        private long _lastIssued;

        public EntityId Next()
        {
            return new EntityId(checked(++_lastIssued));
        }

        // For when a sim is being restored from a serialized state
        // Call with the highest restored entity ID
        public void AdvancePast(EntityId existingId)
        {
            if (existingId.Value > _lastIssued)
                _lastIssued = existingId.Value;
        }
    }
}
