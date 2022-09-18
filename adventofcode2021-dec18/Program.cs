using adventofcode2021_dec18;
using static common.Utils;

var inputLines = GetLines("myPuzzleInput.txt");
Console.WriteLine($"Found {inputLines.Length} lines of input.");

var number = SnailfishNumber.Parse(inputLines[0]);
for (var i = 1; i < inputLines.Length; i++)
{
    var other = SnailfishNumber.Parse(inputLines[i]);
    number = SnailfishNumber.Add(number, other);
}

Console.WriteLine($"final sum is {number} with magnitude {number.OuterPair.GetMagnitude()}.");

var largestFrom2 = DetermineLargestMagnitudeFrom2();

Console.WriteLine($"Largest magnitude from adding any 2 numbers appears to be {largestFrom2}");

long DetermineLargestMagnitudeFrom2() {

    var largestMagnitude = 0L;

    for (var i = 0; i < inputLines.Length; i++)
    {
        for (var j = 0; j < inputLines.Length; j++)
        {
            if (i != j)
            {
                var a = SnailfishNumber.Parse(inputLines[i]);
                var b = SnailfishNumber.Parse(inputLines[j]);
                var sum = SnailfishNumber.Add(a, b);
                var magnitude = sum.OuterPair.GetMagnitude();
                if (magnitude > largestMagnitude)
                {
                    largestMagnitude = magnitude;
                }
            }
        }
    }
    return largestMagnitude;
}