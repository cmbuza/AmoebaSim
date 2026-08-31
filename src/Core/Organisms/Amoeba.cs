using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Numerics;
using AmoebaSim.Core.Genetics;
using AmoebaSim.Core.Primitives;
using AmoebaSim.Core.Simulation;

namespace AmoebaSim.Core.Organisms
{
    public enum Attributes
    {
        SIZE = 0,
        SPEED = 1,
        VIEWDIST = 2,
        NUMOFFSPRING = 3,
        FOODNEEDEDTOREPRODUCE = 4,
        SMELLDIST = 5
    }

    public class Amoeba
    {
        public static bool IsNotAlive(Amoeba o)
        {
            return !o.IsAlive;
        }

        private Genome<IntegerGene> genome;
        private int x, y, plantCount;
        private Vector2 direction;
        private Point targ;
        private bool bAlive;

        //private Timer lifeTimer, switchDirTimer;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int R
        {
            get { return (int)genome.Genes[(int)Attributes.SIZE].Value; }
            set { genome.Genes[(int)Attributes.SIZE].Value = value; }
        }

        public int PlantsEaten
        {
            get { return plantCount; }
            set { plantCount = value; }
        }

        public int Speed
        {
            get { return (int)genome.Genes[(int)Attributes.SPEED].Value; }
            set { genome.Genes[(int)Attributes.SPEED].Value = value; }
        }

        public int ViewDistance
        {
            get { return (int)genome.Genes[(int)Attributes.VIEWDIST].Value; }
            set { genome.Genes[(int)Attributes.VIEWDIST].Value = value; }
        }

        public int SmellDistance
        {
            get { return (int)genome.Genes[(int)Attributes.SMELLDIST].Value; }
            set { genome.Genes[(int)Attributes.SMELLDIST].Value = value; }
        }

        public int NumberOfOffspring
        {
            get { return (int)genome.Genes[(int)Attributes.NUMOFFSPRING].Value; }
            set { genome.Genes[(int)Attributes.NUMOFFSPRING].Value = value; }
        }

        public int FoodNeededToReproduce
        {
            get { return (int)genome.Genes[(int)Attributes.FOODNEEDEDTOREPRODUCE].Value; }
            set { genome.Genes[(int)Attributes.FOODNEEDEDTOREPRODUCE].Value = value; }
        }

        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public Point Target
        {
            get { return targ; }
            set
            {
                targ = value;
                direction = new Vector2(targ.X - x, targ.Y - y);
                direction = Vector2.Normalize(direction);
            }
        }

        public bool IsAlive
        {
            get { return bAlive; }
            set {  bAlive = value; }
        }

        public bool IsInHeat
        {
            get { return PlantsEaten >= FoodNeededToReproduce; }
        }

        public Amoeba()
        {
            genome = new Genome<IntegerGene>(Enum.GetNames(typeof(Attributes)).Length);

            #region gene parameters
            // Parameters for SIZE gene
            genome.Genes[0].MinVal = 10;
            genome.Genes[0].MaxVal = 50;
            genome.Genes[0].Step = 5;

            // Parameters for SPEED gene
            genome.Genes[1].MinVal = 0;
            genome.Genes[1].MaxVal = 10;
            genome.Genes[1].Step = 1;

            // Parameters for VIEWDIST gene
            genome.Genes[2].MinVal = 10;
            genome.Genes[2].MaxVal = 1000;
            genome.Genes[2].Step = 5;

            // Parameters for NUMOFFSPRING gene
            genome.Genes[3].MinVal = 0;
            genome.Genes[3].MaxVal = 100;
            genome.Genes[3].Step = 1;

            // Parameters for FOODNEEDEDTOREPRODUCE gene
            genome.Genes[4].MinVal = 2;
            genome.Genes[4].MaxVal = 100;
            genome.Genes[4].Step = 1;

            // Parameters for SMELLDIST gene
            genome.Genes[5].MinVal = 10;
            genome.Genes[5].MaxVal = 1000;
            genome.Genes[5].Step = 5;
            #endregion

            x = 0;
            y = 0;
            R = 1;

            plantCount = 0;

            bAlive = true;
        }

        public Amoeba(int xVal, int yVal, int radius)
            : this()
        {
            x = xVal;
            y = yVal;
            R = radius;
        }

        public Amoeba(int xVal, int yVal, int radius, int speed, int viewDist, int numOffspring, int foodToReproduce, int smellDist)
            : this(xVal, yVal, radius)
        {
            Speed = speed;
            ViewDistance = viewDist;
            NumberOfOffspring = numOffspring;
            FoodNeededToReproduce = foodToReproduce;
            SmellDistance = smellDist;
        }

        public Amoeba(Amoeba o)
            : this()
        {
            genome = new Genome<IntegerGene>(o.genome);
            x = o.x;
            y = o.y;
        }

        public override string ToString()
        {
            return genome.ToString();
        }

        internal void Move(AmoebaSimContext context)
        {
            x += (int)Math.Round(Speed * direction.X);
            if (x > context.WorldWidth || x < 0)
                direction.X = -direction.X;

            y += (int)Math.Round(Speed * direction.Y);
            if (y > context.WorldHeight || y < 0)
                direction.Y = -direction.Y;
        }

        private double ageSeconds = 0;
        private double directionTimerSeconds;
        private double nextDirectionChangeSeconds;

        internal void Update(double deltaSeconds, AmoebaSimContext context)
        {
            ageSeconds += deltaSeconds;

            if (ageSeconds >= context.Config.AmoebaLifeSpan.TotalSeconds)
            {
                IsAlive = false;
                return;
            }

            directionTimerSeconds += deltaSeconds;
            if (directionTimerSeconds >= nextDirectionChangeSeconds)
            {
                Target = new Point(context.Random.Next(context.WorldWidth),
                    context.Random.Next(context.WorldHeight));
                directionTimerSeconds = 0;
                nextDirectionChangeSeconds = context.Random.Next(5, 15);
            }

            Move(context);
        }

        internal int InteractsWithPlant(Plant p, AmoebaSimContext context)
        {
            Vector2 vec = new Vector2(x - p.Location.X, y - p.Location.Y);

            int ret = 0; //0 means no interaction

            if (vec.Length() > ViewDistance + p.Radius
                && vec.Length() <= SmellDistance + p.Radius)
            {
                Random r = context.Random;
                int num = r.Next(SmellDistance + p.Radius);
                if (num > vec.Length())
                    if (r.Next(5) == 1)
                        return 1;
            }

            if (vec.Length() <= ViewDistance + p.Radius)
            {
                ret++; //1 means the plant can be seen
                if (vec.Length() <= R + p.Radius)
                    ret++; //2 means the plant is being touched
            }

            return ret;
        }

        internal int InteractsWithOrganism(Amoeba o, AmoebaSimContext context)
        {
            Vector2 vec = new Vector2(x - o.X, y - o.Y);

            int ret = 0; //0 means no interaction

            if (vec.Length() > ViewDistance + o.R
                && vec.Length() <= SmellDistance + o.SmellDistance)
            {
                Random r = context.Random;
                int num = r.Next(SmellDistance + o.SmellDistance);
                if (num > vec.Length())
                    if (r.Next(5) == 1)
                        return 1;
            }

            if (vec.Length() <= ViewDistance + o.R)
            {
                ret++; //1 means the organism can be seen
                if (vec.Length() <= R + o.R)
                    ret++; //2 means the organism is being touched
            }

            return ret;
        }

        internal List<Amoeba> Reproduce(AmoebaSimContext context)
        {
            List<Amoeba> children = new List<Amoeba>();
            for (int i = 0; i < NumberOfOffspring; i++)
            {
                Amoeba o = new Amoeba(this);
                o.genome.MutateAll(context.Config.MutationProbability, context.Random);
                children.Add(o);
            }

            return children;
        }

        internal List<Amoeba> Reproduce(Amoeba o, AmoebaSimContext context)
        {
            Genome<IntegerGene> newGenome = genome.Crossover(o.genome, context.Random);
            List<Amoeba> children = new List<Amoeba>();
            int numOffspring = (int)newGenome.Genes[(int)Attributes.NUMOFFSPRING].Value;

            for (int i = 0; i < numOffspring; i++)
            {
                Amoeba norg = new Amoeba(this);
                norg.genome = genome.Crossover(o.genome, context.Random);
                norg.genome.MutateAll(context.Config.MutationProbability, context.Random);
                children.Add(norg);
            }

            return children;
        }
    }
}
