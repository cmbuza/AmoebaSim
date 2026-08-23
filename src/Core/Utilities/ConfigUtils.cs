using AmoebaSim.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmoebaSim.Core.Utilities
{
    public class ConfigUtils
    {
        public static AmoebaSimConfig LoadConfig(string configFile) { return new AmoebaSimConfig(); }
        public static void SaveConfig(AmoebaSimConfig config, string configFile) { }
    }
}
