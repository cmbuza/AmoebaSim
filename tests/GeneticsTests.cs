using AmoebaSim.Core.Genetics;

namespace AmoebaSim.Tests;

public class GeneticsTests
{
    [Theory]
    [InlineData(0, -2, 2)]
    [InlineData(-4, -5, -2)]
    [InlineData(4, 2, 5)]
    public void IntegerMutationStepsInEitherDirectionAndClampsAtTheBounds(
        int initial, int lowerOutcome, int upperOutcome)
    {
        var gene = new IntegerGene { MinVal = -5, MaxVal = 5, Step = 2 };
        var random = new Random(42);
        var outcomes = new HashSet<int>();

        for (int i = 0; i < 32; i++)
        {
            gene.Value = initial;
            gene.Mutate(random);
            outcomes.Add((int)gene.Value);
        }

        Assert.Equal(new[] { lowerOutcome, upperOutcome }, outcomes.Order());
    }

    [Fact]
    public void GenomeCopyPreservesEachGeneAndItsMutationSettingsIndependently()
    {
        var parent = new Genome<IntegerGene>(2);
        parent.Genes[0] = new IntegerGene(15) { MinVal = 10, MaxVal = 50, Step = 5 };
        parent.Genes[1] = new IntegerGene(-2) { MinVal = -10, MaxVal = 0, Step = 2 };

        var copy = new Genome<IntegerGene>(parent);

        Assert.Equal(parent.Genes.Count, copy.Genes.Count);
        for (int i = 0; i < parent.Genes.Count; i++)
        {
            var original = parent.Genes[i];
            var inherited = copy.Genes[i];
            Assert.NotSame(original, inherited);
            Assert.Equal(original.Value, inherited.Value);
            Assert.Equal(original.MinVal, inherited.MinVal);
            Assert.Equal(original.MaxVal, inherited.MaxVal);
            Assert.Equal(original.Step, inherited.Step);
        }
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(3, true)]
    public void MutateAllAppliesOneStepToSelectedGenes(int oneIn, bool expectUnchangedGenes)
    {
        var genome = new Genome<IntegerGene>(32);
        foreach (var gene in genome.Genes)
            gene.Step = 3;

        genome.MutateAll(oneIn, new Random(17));

        var values = genome.Genes.Select(gene => (int)gene.Value).ToArray();
        Assert.All(values, value => Assert.Contains(value, new[] { -3, 0, 3 }));
        Assert.Contains(values, value => value != 0);
        Assert.Equal(expectUnchangedGenes, values.Contains(0));
    }

    [Fact]
    public void CrossoverInheritsCorrespondingValuesFromBothParentsInIndependentGenes()
    {
        var first = new Genome<IntegerGene>(16);
        var second = new Genome<IntegerGene>(16);
        for (int i = 0; i < first.Genes.Count; i++)
        {
            first.Genes[i].Value = i + 1;
            second.Genes[i].Value = -(i + 1);
        }

        var child = first.Crossover(second, new Random(42));

        Assert.Equal(first.Genes.Count, child.Genes.Count);
        for (int i = 0; i < child.Genes.Count; i++)
        {
            var gene = child.Genes[i];
            Assert.NotSame(first.Genes[i], gene);
            Assert.NotSame(second.Genes[i], gene);
            Assert.Contains((int)gene.Value, new[] { i + 1, -(i + 1) });
        }
        Assert.Contains(child.Genes, gene => (int)gene.Value > 0);
        Assert.Contains(child.Genes, gene => (int)gene.Value < 0);
    }
}
