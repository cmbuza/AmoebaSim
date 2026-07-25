using System;
using System.Collections.Generic;
using System.Text;

namespace AmoebaSim.Core.Simulation
{
    public class AmoebaSim
    {
        private AmoebaSimConfig simConfig; 
        private AmoebaSimContext simContext;
        public AmoebaSim()
        { 
            simConfig = new AmoebaSimConfig();
            simContext = new AmoebaSimContext();
        }

        public AmoebaSimConfig Config { get { return simConfig; } }
        public AmoebaSimContext Context { get { return simContext; } }
    }
}
