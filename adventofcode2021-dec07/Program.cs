using System.Diagnostics;
using static common.Utils;
using MathNet.Numerics.Statistics;

Console.WriteLine("Day 7: The Treachery of Whales");

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var part1answer = GetOptimalHorizontalPositionWithLinearFuelUse(inputTextFileName);
Debug.Assert(part1answer == 344605);

var part2answer = GetOptimalHorizontalPositionFuelUsedWithScalingFuelUsage(inputTextFileName);
Debug.Assert(part2answer == 93699985);


int GetOptimalHorizontalPositionWithLinearFuelUse(string file)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 7 Part 1 using {file}.");

    var textLines = GetLines(file);
    var crabPositions = textLines.Single().Split(',').Select(x => int.Parse(x)).ToList();
    var optimalPosition = (int)Math.Round(crabPositions.Select(p => (double)p).Median(), 0);
    
    var fuelUsed = crabPositions.Select(p => Math.Abs(p - optimalPosition)).Sum();

    return fuelUsed;
}

int GetOptimalHorizontalPositionFuelUsedWithScalingFuelUsage(string file)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 7 Part 2 using {file}.");

    var textLines = GetLines(file);
    var crabPositions = textLines.Single().Split(',').Select(x => int.Parse(x)).ToList();

    var min = crabPositions.Min();
    var max = crabPositions.Max();
    
    var fuelUsageForDistance = PrecalculateFuelUseForDistances(max - min);

    var fuelUsages = new List<(int position, int fuelUsed)>();
    for (var pos = min; pos <= max; pos++)
    {
        fuelUsages.Add((position: pos, fuelUsed: crabPositions.Select(p => fuelUsageForDistance[Math.Abs(p - pos)]).Sum()));
    }

    return fuelUsages.MinBy(u => u.fuelUsed).fuelUsed;

    static Dictionary<int, int> PrecalculateFuelUseForDistances(int maxTravelDistance)
    {
        var fuelUsageForDistance = new Dictionary<int, int>(maxTravelDistance + 1)
        {
            { 0, 0 }
        };

        for (var d = 1; d <= maxTravelDistance; d++)
        {
            fuelUsageForDistance.Add(d, fuelUsageForDistance[d - 1] + d);
        }

        return fuelUsageForDistance;
    }
}