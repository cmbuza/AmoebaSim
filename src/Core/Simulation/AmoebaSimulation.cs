using AmoebaSim.Core.Organisms;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AmoebaSim.Core.Simulation
{
    public sealed class AmoebaSimulation
    {
        private double _plantGrowAccumulator = 0.0;

        private readonly AmoebaSimContext _context;
        private readonly List<Amoeba> _amoebas = [];
        private readonly List<Plant> _plants = [];
        //private readonly SpatialHash<ISpatialEntity> _spatialIndex = [];

        public AmoebaSimConfig Config { get; }

        public IReadOnlyList<Amoeba> Amoebas => _amoebas;

        public IReadOnlyList<Plant> Plants => _plants;

        public long Tick { get; private set; }

        public bool IsExtinct => _amoebas.Count == 0;

        public AmoebaSimulation(AmoebaSimConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            config.Validate();

            Config = config;
            _context = new AmoebaSimContext(config);

            Reset();
        }

        public void Reset()
        {
            _amoebas.Clear();
            _plants.Clear();

            Tick = 0;
            _plantGrowAccumulator = 0.0;

            for (int i = 0; i < Config.InitialAmoebaCount; i++)
            {
                _amoebas.Add(
                    new Amoeba(
                        Random.Shared.Next(Config.WorldWidth),
                        Random.Shared.Next(Config.WorldHeight),
                        Random.Shared.Next(10, 50),
                        1,
                        Random.Shared.Next(20, 100),
                        Random.Shared.Next(1, 5),
                        Random.Shared.Next(2, 7),
                        Random.Shared.Next(30, 100)));
            }

            Grow(Config.PlantsPerGrowth);
        }

        private TimeSpan _advanceAccumulator = TimeSpan.Zero;

        public void Advance(TimeSpan elapsed)
        {
            if (elapsed < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(elapsed));

            _advanceAccumulator += elapsed;

            while (_advanceAccumulator >= Config.FixedStep)
            {
                Step();
                _advanceAccumulator -= Config.FixedStep;
            }
        }

        public void Step()
        {
            double elapsed = Config.FixedStep.TotalSeconds;
            int iwp = 0, iwo = 0;

            List<Amoeba> newOrganisms = new List<Amoeba>();

            foreach (Amoeba o in _amoebas)
            {
                try
                {

                    if (o.IsAlive)
                    {
                        o.Update(elapsed, _context);

                        if (o.IsInHeat)
                        {
                            foreach (Amoeba org in _amoebas)
                            {
                                if (!org.IsAlive) continue;
                                if (org == o) continue;
                                if (!org.IsInHeat) continue;

                                iwo = o.InteractsWithOrganism(org, _context);
                                if (iwo == 0) continue;

                                if (iwo == 1)
                                    o.Target = new Core.Primitives.Point(org.X, org.Y);
                                else
                                {
                                    o.PlantsEaten = 0;
                                    org.PlantsEaten = 0;
                                    newOrganisms.AddRange(o.Reproduce(org, _context));
                                }
                            }
                        }
                        else
                        {
                            foreach (Plant p in _plants)
                            {
                                if (p.Eaten) continue;

                                iwp = o.InteractsWithPlant(p, _context);
                                if (iwp == 0) continue;

                                if (iwp == 1)
                                    o.Target = p.Location;
                                else
                                {
                                    p.Eaten = true;
                                    o.PlantsEaten++;
                                }
                            }
                        }
                    }
                }
                catch (InvalidOperationException ioe)
                {
                    System.Diagnostics.Debug.WriteLine(ioe);
                }
            }

            _amoebas.RemoveAll(Amoeba.IsNotAlive);
            _amoebas.AddRange(newOrganisms);
            _plants.RemoveAll(Plant.IsEaten);

            _plantGrowAccumulator += elapsed;

            if (_plantGrowAccumulator >= Config.PlantGrowthInterval.TotalSeconds)
            {
                Grow(Config.PlantsPerGrowth);
                _plantGrowAccumulator = 0.0;
            }

            Tick++;
        }

        private void Grow(int numToGrow)
        {
            for (int i = 0; i < numToGrow; i++)
                _plants.Add(new Plant(
                        _context.Random.Next(_context.WorldWidth),
                        _context.Random.Next(_context.WorldHeight),
                        _context.Random.Next(5, 30)
                        )
                    );
        }
    }
}
