using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AmoebaSim.Desktop
{
    public class Genome<T> where T : IGene, new()
    {
        private List<T> genes;

        public List<T> Genes
        {
            get { return genes; }
            set { genes = value; }
        }

        public Genome()
        {
            genes = new List<T>(1);
            genes.Add(new T());
        }

        public Genome(int size)
        {
            genes = new List<T>(size);
            for (int i = 0; i < size; i++)
                genes.Add(new T());
        }

        public Genome(Genome<T> original)
        {
            genes = new List<T>(original.Genes.Count);
            for (int i = 0; i < genes.Capacity; i++)
                genes.Add(new T());
            for (int j = 0; j < genes.Count; j++)
            {
                genes[j].Value = original.Genes[j].Value;
                genes[j].MaxVal = original.Genes[j].MaxVal;
                genes[j].MinVal = original.Genes[j].MinVal;
                genes[j].Step = original.Genes[j].Step;
            }
        }

        public void MutateAll(int probabilityOfMutation) // Read as 1 in probabilityOfMutation
        {
            Random r = new Random((int)DateTime.UtcNow.Ticks);

            foreach (T g in genes)
                if (r.Next(probabilityOfMutation) == r.Next(probabilityOfMutation))
                    g.Mutate();
        }

        public Genome<T> Crossover(Genome<T> genome)
        {
            Random r = new Random((int)DateTime.Now.Ticks);
            Genome<T> newGenome = new Genome<T>(this);
            for(int i = 0; i < newGenome.genes.Count; i++)
            {
                if (r.Next() % 2 == 0)
                    newGenome.genes[i].Value = genome.Genes[i].Value;
            }
            return newGenome;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (T g in genes)
                sb.Append("[" + g.ToString() + "]");
            return sb.ToString();
        }
    }
}
