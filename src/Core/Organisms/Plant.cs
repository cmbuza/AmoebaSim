using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmoebaSim.Core.Primitives;

namespace AmoebaSim.Core.Organisms
{
    public sealed class Plant : ISpatialEntity
    {
        private EntityId _id;

        public static bool IsEaten(Plant p)
        {
            return p.Eaten;
        }

        private Point loc;
        public Point Location
        {
            get { return loc; }
            set { loc = value; }
        }

        private bool isEaten;
        public bool Eaten
        {
            get { return isEaten; }
            set { isEaten = value; }
        }

        #region ISpatialEntity

        public EntityId Id => _id;

        public Point Position => Location;

        private int size;
        private static int DEFAULT_RADIUS = 10;
        public int Radius
        {
            get { return size; }
            set { size = value; }
        }

        public SpatialEntityKind Kind => SpatialEntityKind.Plant;

        #endregion

        public Plant(EntityId id, int x, int y, int rad)
        {
            _id = id;
            loc = new Point(x, y);

            size = rad;

            isEaten = false;
        }

        public Plant(EntityId id) : this(id, 0, 0, DEFAULT_RADIUS) {}

        public Plant(EntityId id, int x, int y) : this(id, x, y, DEFAULT_RADIUS) {}

    }
}
