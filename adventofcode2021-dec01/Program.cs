using System.Diagnostics;
using static common.Utils;

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput1.txt";

var part1Answer = PrintDayOnePartOneAnswer(inputTextFileName);

Debug.Assert(part1Answer == 1553);

var part2Answer = PrintDayOnePartTwoAnswer(inputTextFileName);

Debug.Assert(part2Answer == 1597);

int PrintDayOnePartOneAnswer(string inputFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 1 Part One using {inputFileName}.");
    var depthMeasurements = GetInputAsIntegers(inputFileName).ToArray();
    var depthIncreases = NumberOfTimesDepthIncreases(depthMeasurements);
    Console.WriteLine($"The depth increases {depthIncreases} times.");
    return depthIncreases;
}

int PrintDayOnePartTwoAnswer(string inputFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 1 Part Two using {inputFileName}.");
    var depthMeasurements = GetInputAsIntegers(inputFileName).ToArray();
    var depthIncreases = NumberOfTimesDepthIncreasesSlidingWindow(depthMeasurements, 3);
    Console.WriteLine($"The depth increases {depthIncreases} times.");
    return depthIncreases;
}

int NumberOfTimesDepthIncreasesSlidingWindow(int[] measurements, int windowSize)
{
    const int windowOffset = 1;
    if (measurements.Length < windowSize + windowOffset)
    {
        return 0;
    }
    var result = 0;
    var current = measurements.Take(windowSize).Sum();
    for (var i = 0; i < measurements.Length - windowSize + windowOffset; i++)
    {
        var next = measurements.Skip(i + windowOffset).Take(windowSize).Sum();
        if (current < next)
        {
            result++;
        }
        current = next;
    }
    return result;
}

int NumberOfTimesDepthIncreases(int[] measurements)
{
    var result = 0;
    for (var i = 0; i < measurements.Length - 1; i++)
    {
        if (measurements[i] < measurements[i + 1])
        {
            result++;
        }
    }
    return result;
}
