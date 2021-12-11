using System.Diagnostics;
using static common.Utils;

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";


var part1Answer = PrintDay6AnswerMoreScalable(inputTextFileName, 80);
Debug.Assert(part1Answer == 350605);
var part2Answer = PrintDay6AnswerMoreScalable(inputTextFileName, 256);
Debug.Assert(part2Answer == 1592778185024);

long PrintDay6Answer(string file, int daysToSimulate)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 6 Part 1 using {file}.");
    Console.WriteLine("Lanternfish");

    var textLines = GetLines(file);
    var fish = textLines.Single().Split(',').Select(x => int.Parse(x)).ToList();

    Console.WriteLine("Initial State: " + string.Join(",", fish));
    for (var day = 1; day <= daysToSimulate; day++)
    {
        var newFishCount = 0;
        for (var i = 0; i < fish.Count; i++)
        {
            var newValue = fish[i] - 1;
            if (newValue < 0)
            {
                fish[i] = 6;
                newFishCount++;
            }
            else
            {
                fish[i] = newValue;
            }
        }
        fish.AddRange(Enumerable.Repeat(8, newFishCount));
        Console.WriteLine($"After {day} days there are {fish.Count} fish.");
    }

    Console.WriteLine($"The answer for part one is {fish.Count} fish after simulating {daysToSimulate} days.");
    return fish.Count;
}

long PrintDay6AnswerMoreScalable(string file, int daysToSimulate)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 6 Part 1 using {file}.");
    Console.WriteLine("Lanternfish");

    var textLines = GetLines(file);
    var fish = textLines.Single().Split(',').Select(x => int.Parse(x)).Aggregate(
        new Dictionary<int, long>(), (acc, next) => {
            if (!acc.TryAdd(next, 1))
            {
                acc[next] += 1;
            }
            return acc;
        });

    Console.WriteLine("Initial State: " + string.Join(",", fish));

    for (var day = 1; day <= daysToSimulate; day++)
    {
        for (var key = 0; key <= 8; key++)
        {
            if (fish.TryGetValue(key, out var count))
            {
                fish[key - 1] = count;
            }
            else
            {
                fish[key - 1] = 0;
            }
        }
        if (fish.TryGetValue(-1, out var newFishCount))
        {
            if (!fish.TryAdd(6, newFishCount))
            {
                fish[6] += newFishCount;
                fish[8] = newFishCount;
                fish[-1] = 0;
            }
        }
        Console.WriteLine($"After {day} days there are {fish.Values.Sum()} fish.  {string.Join(",", fish)}");
    }

    Console.WriteLine($"The answer for more scalable is {fish.Values.Sum()} fish after simulating {daysToSimulate} days.");
    return fish.Values.Sum();
}