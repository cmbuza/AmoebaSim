using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AmoebaSim.Desktop
{
    public class Plant
    {
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

        private int size;
        public int Radius
        {
            get { return size; }
            set { size = value; }
        }

        private bool isEaten;
        public bool Eaten
        {
            get { return isEaten; }
            set { isEaten = value; }
        }

        public Plant()
        {
            int x = Main.Rand.Next(Main.BACKBUFFER_WIDTH);
            int y = Main.Rand.Next(Main.BACKBUFFER_HEIGHT);
            loc = new Point(x,y);

            size = Main.Rand.Next(5, 30);

            isEaten = false;
        }
    }
}
