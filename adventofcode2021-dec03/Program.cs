using common.BitArrayExtensionMethods;
using System.Collections;
using System.Diagnostics;
using static common.Utils;

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var part1Answer = PrintDay3PartOneAnswer(inputTextFileName);
Debug.Assert(part1Answer == 2498354);

var part2Answer = PrintDay3PartTwoAnswer(inputTextFileName);
Debug.Assert(part2Answer == 3277956);

uint PrintDay3PartOneAnswer(string inputTextFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 3 Part One using {inputTextFileName}.");
    Console.WriteLine("Binary Diagnostic");

    var bits = GetLines(inputTextFileName)
       .Select(s => {
           var parsed = s.TryParseToBitArray(out var bits);
           if (bits is null || !parsed)
           {
               throw new FormatException($"Could not parse {s} to a {nameof(BitArray)}");
           }
           return bits;
       })
       .ToList();

    var gamma = GetGammaBits(bits).ToUInt32();
    var epsilon = GetEpsilonBits(bits).ToUInt32();
    var product = gamma * epsilon;
    Console.WriteLine($"The Gamma Rate is {gamma} and Epsilon Rate is {epsilon}.  The product is {product}.");

    return product;
}

uint PrintDay3PartTwoAnswer(string inputTextFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 3 Part Two using {inputTextFileName}.");
    Console.WriteLine("Binary Diagnostic");

    var bits = GetLines(inputTextFileName)
       .Select(s => {
           var parsed = s.TryParseToBitArray(out var bits);
           if (bits is null || !parsed)
           {
               throw new FormatException($"Could not parse {s} to a {nameof(BitArray)}");
           }
           return bits;
       })
       .ToArray();

    var oxygenGeneratorRating = GetOxygenRating(bits).ToUInt32();
    var co2ScrubberRating = GetCO2ScrubberRating(bits).ToUInt32();

    var lifeSupportRating = oxygenGeneratorRating * co2ScrubberRating;

    Console.WriteLine($"The O2 Generator Rating is {oxygenGeneratorRating} and the CO2 Scrubber Rating is {co2ScrubberRating}.  The life support rating is {lifeSupportRating}.");

    return lifeSupportRating;
}

BitArray GetOxygenRating(BitArray[] logItems) =>
    GetLifeSupportRating(logItems, (trueCount, falseCount) => trueCount < falseCount);

BitArray GetCO2ScrubberRating(BitArray[] logItems) =>
    GetLifeSupportRating(logItems, (trueCount, falseCount) => trueCount >= falseCount);


BitArray GetLifeSupportRating(BitArray[] logItems, Func<int, int, bool> criteriaForRemoval)
{
    // unfortunately we don't know that the data will be "rectangular",
    // so we have to prove that it is so we can use it safely.
    var width = GetConsistentCountOrThrow(logItems);

    var interestingLogItems = logItems.ToList();
    for (var bitIndex = 0; bitIndex < width; bitIndex++)
    {
        var (trueCount, falseCount) = CountBits(interestingLogItems, bitIndex);
        var removeWhere = criteriaForRemoval(trueCount, falseCount);
        interestingLogItems.RemoveAll(x => x[bitIndex] == removeWhere);
        if (interestingLogItems.Count == 1)
        {
            break;
        }
    }

    return interestingLogItems.Single();
}

(int trueCount, int falseCount) CountBits(List<BitArray> logItems, int bitIndex)
{
    var trueCount = 0;
    var falseCount = 0;

    for (var j = 0; j < logItems.Count; j++)
    {
        if (logItems[j][bitIndex])
        {
            trueCount++;
        }
        else
        {
            falseCount++;
        }
    }
    return (trueCount, falseCount);
}

BitArray GetEpsilonBits(List<BitArray> logItems)
{
    // unfortunately we don't know that the data will be "rectangular",
    // so we have to prove that it is so we can use it safely.
    var width = GetConsistentCountOrThrow(logItems);

    var bits = new BitArray(width, true);
    for (var i = 0; i < width; i++)
    {
        var (trueCount, falseCount) = CountBits(logItems, i);
        if (falseCount < trueCount)
        {
            bits.Set(i, false);
        }
    }
    return bits;
}

BitArray GetGammaBits(List<BitArray> logItems)
{
    // unfortunately we don't know that the data will be "rectangular",
    // so we have to prove that it is so we can use it safely.
    var width = GetConsistentCountOrThrow(logItems);

    var bits = new BitArray(width, false);
    for (var i = 0; i < width; i++)
    {
        var (trueCount, falseCount) = CountBits(logItems, i);
        if (trueCount > falseCount)
        {
            bits.Set(i, true);
        }
    }
    return bits;
}

int GetConsistentCountOrThrow(IEnumerable<ICollection> items)
{
    using var e = items.GetEnumerator();
    if (!e.MoveNext())
    {
        throw new Exception("No way to determine consistent count with empty.");
    }
    var count = e.Current.Count;
    while (e.MoveNext())
    {
        if (e.Current.Count != count)
        {
            throw new Exception($"inconsisent count {e.Current.Count} found - was expecting {count}.");
        }
    }
    return count;
}