namespace AmoebaSim.Core.Spatial
{

    public sealed class SpatialHash<T>
        where T : class, ISpatialEntity
    {
        private readonly record struct CellCoordinate(long X, long Y);

        private readonly int _cellSize;
        private readonly Dictionary<CellCoordinate, List<T>> _cells = [];
        private readonly HashSet<EntityId> _seen = [];

        public SpatialHash(int cellSize)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cellSize);
            _cellSize = cellSize;
        }

        public void Clear()
        {
            _cells.Clear();
            _seen.Clear();
        }

        public void Add(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (!entity.Id.IsAssigned)
            {
                throw new ArgumentException(
                    "The entity must have an assigned ID.",
                    nameof(entity));
            }

            foreach (CellCoordinate coordinate in GetCells(entity.Bounds))
            {
                if (!_cells.TryGetValue(coordinate, out List<T>? cell))
                {
                    cell = [];
                    _cells.Add(coordinate, cell);
                }

                cell.Add(entity);
            }
        }

        public void Query(
            SpatialBounds area,
            SpatialEntityKind kinds,
            List<T> results,
            EntityId excludedId = default)
        {
            ArgumentNullException.ThrowIfNull(results);

            results.Clear();
            _seen.Clear();

            foreach (CellCoordinate coordinate in GetCells(area))
            {
                if (!_cells.TryGetValue(coordinate, out List<T>? cell))
                    continue;

                foreach (T entity in cell)
                {
                    if (entity.Id == excludedId)
                        continue;

                    if ((entity.Kind & kinds) == 0)
                        continue;

                    if (!_seen.Add(entity.Id))
                        continue;

                    if (!entity.Bounds.Intersects(area))
                        continue;

                    results.Add(entity);
                }
            }

            results.Sort(static (left, right) =>
                left.Id.Value.CompareTo(right.Id.Value));
        }

        private IEnumerable<CellCoordinate> GetCells(SpatialBounds bounds)
        {
            if (bounds.MinX > bounds.MaxX || bounds.MinY > bounds.MaxY)
            {
                throw new ArgumentException(
                    "Minimum bounds must not exceed maximum bounds.",
                    nameof(bounds));
            }

            long minX = ToCell(bounds.MinX);
            long maxX = ToCell(bounds.MaxX);
            long minY = ToCell(bounds.MinY);
            long maxY = ToCell(bounds.MaxY);

            for (long y = minY; ; y++)
            {
                for (long x = minX; ; x++)
                {
                    yield return new CellCoordinate(x, y);

                    if (x == maxX)
                        break;
                }

                if (y == maxY)
                    break;
            }
        }

        private long ToCell(long coordinate)
        {
            long quotient = coordinate / _cellSize;

            // Integer division truncates toward zero; we need floor division.
            return coordinate % _cellSize < 0
                ? quotient - 1
                : quotient;
        }
    }
}
