using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmoebaSim.Desktop
{
    public class IntegerGene : IGene
    {
        private int val, min, max, step;

        public IntegerGene()
        {
            val = 0;
            min = int.MinValue;
            max = int.MaxValue;
            step = 1;
        }

        public IntegerGene(int v)
            : this()
        {
            val = v;
        }

        public object MinVal
        {
            get { return min; }
            set { min = (int)value; if (val < min) val = min; }
        }

        public object MaxVal
        {
            get { return max; }
            set { max = (int)value; if (val > max) val = max; }
        }

        public object Step
        {
            get { return step; }
            set { step = (int)value; }
        }

        #region IGene Members

        public object Value
        {
            get { return val; }
            set { val = (int)value; }
        }

        public void Mutate()
        {
            Random r = new Random((int)DateTime.UtcNow.Ticks);
            int b = r.Next(2);

            if (b != 0)
                val += step;
            else
                val -= step;

            if (val < min)
                val = min;
            else if (val > max)
                val = max;
        }

        #endregion

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
