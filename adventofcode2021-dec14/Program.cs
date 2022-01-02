using System.Diagnostics;
using static common.Utils;

Console.WriteLine("Day 14: Extended Polymerization");

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var lines = GetLines(inputTextFileName);

var answer1 = DoPart1(lines);
Debug.Assert(answer1 == 3406);

var answer2 = DoPart2(lines);
Debug.Assert(answer2 == 3941782230241L);

long DoPart1(string[] lines)
{
    var p = BigPolymer.Parse(lines);
    for (var i = 0; i < 10; i++)
    {
        p.Step();
    }

    var counts = p.GetElementCounts().OrderByDescending(x => x.Count).ToArray();

    return counts[0].Count - counts[^1].Count;
}

long DoPart2(string[] lines)
{
    var p = BigPolymer.Parse(lines);
    for (var i = 0; i < 40; i++)
    {
        p.Step();
    }

    var counts = p.GetElementCounts().OrderByDescending(x => x.Count).ToArray();

    return counts[0].Count - counts[^1].Count;
}


public struct Pair
{
    public char Left { get; set; }
    public char Right { get; set; }
    public override string ToString()
    {
        return new string(new char[] { Left, Right });
    }
}

public class BigPolymer
{
    public string Template { get; set; }
    public int StepCount { get; set; }
    public Dictionary<Pair, long> Pairs { get; set;}

    public Dictionary<Pair, char> PairRules { get; set; }

    public IEnumerable<(char Element, long Count)> GetElementCounts()
    {
        var result = Pairs.Select(kvp => (Element: kvp.Key.Left, Count: kvp.Value))
            .GroupBy(kvp => kvp.Element)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

        var last = Template[^1];
        if (!result.TryAdd(last, 1L))
        {
            result[last] += 1L;
        }

        return result.Select(kvp => (Element: kvp.Key, Count: kvp.Value));
    }
    
    public long PolymerLength() => Pairs.Sum(p => p.Value) + 1L;

    public void Step()
    {
        var newPairs = new Dictionary<Pair, long>();
        var p = new Pair();
        foreach (var oldPair in Pairs)
        {
            var rule = PairRules[oldPair.Key];

            p.Left = oldPair.Key.Left;
            p.Right = rule;
            if (!newPairs.TryAdd(p, oldPair.Value))
            {
                newPairs[p] += oldPair.Value;
            }

            p.Left = rule;
            p.Right = oldPair.Key.Right;
            if (!newPairs.TryAdd(p, oldPair.Value))
            {
                newPairs[p] += oldPair.Value;
            }
        }

        StepCount += 1;
        Pairs = newPairs;
    }

    public static BigPolymer Parse(string[] lines)
    {
        var polymer = Polymer.Parse(lines);
        var result = new BigPolymer
        {
            Template = polymer.Template,
            StepCount = polymer.StepCount,
            PairRules = polymer.PairRules,
            Pairs = new()
        };
        var lookupPair = new Pair();

        for (var i = 0; i < polymer.Current.Length - 1; i++)
        {
            lookupPair.Left = polymer.Current[i];
            lookupPair.Right = polymer.Current[i+1];

            if (!result.Pairs.TryAdd(lookupPair, 1L))
            {
                result.Pairs[lookupPair]++;
            }
        }

        return result;
    }
}

public class Polymer
{
    public string Template { get; set; }
    public int StepCount { get; set; }
    public char[] Current { get; set; }
    public Dictionary<Pair, char> PairRules { get; set; }

    public IEnumerable<(char Element, long Count)> GetElementCounts() =>
        Current.GroupBy(c => c).Select(g => (Element: g.Key, Count: g.LongCount()));

    public void Step()
    {
        var next = new char[Current.Length + Current.Length - 1];
        var nextIndex = 0;
        var lookupPair = new Pair();
        for (var i = 0; i < Current.Length - 1; i++)
        {
            next[nextIndex] = Current[i];
            nextIndex++;
            lookupPair.Left = Current[i];
            lookupPair.Right = Current[i + 1];

            if (PairRules.TryGetValue(lookupPair, out var insertion))
            {
                next[nextIndex] = insertion;
                nextIndex++;
            }
        }
        next[^1] = Current[^1];
        StepCount += 1;
        Current = next;
    }

    public static Polymer Parse(string[] lines)
    {
        var result = new Polymer();
        result.Template = lines[0];
        result.PairRules = new Dictionary<Pair, char>(lines.Length - 1);
        for (var i = 1; i < lines.Length; i++)
        {
            result.PairRules.Add(new Pair() { Left = lines[i][0], Right = lines[i][1] }, lines[i][^1]);
        }
        result.StepCount = 0;
        result.Current = result.Template.ToCharArray();
        return result;
    }
}
