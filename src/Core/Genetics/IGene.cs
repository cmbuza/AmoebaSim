using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmoebaSim.Core.Genetics
{
    public interface IGene
    {
        object Value { get; set; }

        object MinVal { get; set; }

        object MaxVal { get; set; }

        object Step { get; set; }

        void Mutate();
    }
}
