using common.BitArrayExtensionMethods;
using System.Collections;
using System.Diagnostics;
using static common.Utils;

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var part1Answer = PrintDay3PartOneAnswer(inputTextFileName);
Debug.Assert(part1Answer == 2498354); // correct answer but fairly certain could have been arrived at better.

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
       .ToArray();

    var gamma = GetGammaBits(bits).ToUInt32();
    var epsilon = GetEpsilonBits(bits).ToUInt32();
    var product = gamma * epsilon;
    Console.WriteLine($"The Gamma Rate is {gamma} and Epsilon Rate is {epsilon}.  The product is {product}.");

    return product;
}

BitArray GetEpsilonBits(BitArray[] arrays)
{
    // unfortunately we don't know that the data will be "rectangular",
    // so we have to prove that it is so we can use it safely.
    var width = GetConsistentCountOrThrow(arrays);

    var bits = new BitArray(width, true);
    for (var i = 0; i < width; i++)
    {
        var trueCount = 0;
        var falseCount = 0;

        for (var j = 0; j < arrays.Length; j++)
        {
            if (arrays[j][i])
            {
                trueCount++;
            }
            else
            {
                falseCount++;
            }
        }
        if (falseCount < trueCount)
        {
            bits.Set(i, false);
        }
    }
    return bits;
}

BitArray GetGammaBits(BitArray[] arrays)
{
    // unfortunately we don't know that the data will be "rectangular",
    // so we have to prove that it is so we can use it safely.
    var width = GetConsistentCountOrThrow(arrays);

    var bits = new BitArray(width, false);
    for (var i = 0; i < width; i++)
    {
        var trueCount = 0;
        var falseCount = 0;

        for (var j = 0; j < arrays.Length; j++)
        {
            if(arrays[j][i])
            {
                trueCount++;
            }
            else
            {
                falseCount++;
            }
        }
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