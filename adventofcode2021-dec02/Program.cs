using System.Diagnostics;
using static common.Utils;

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var part1Answer = PrintDayTwoPartOneAnswer(inputTextFileName);
Debug.Assert(part1Answer == 1383564);

var part2Answer = PrintDayTwoPartTwoAnswer(inputTextFileName);
Debug.Assert(part2Answer == 1488311643);


int PrintDayTwoPartOneAnswer(string inputFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 2 Part One using {inputFileName}.");
    Console.WriteLine("Dive!");

    var result = GetLines(inputFileName)
        .AsParallel()
        // don't need AsOrdered here since we just want to know the end position and it's sums only.
        .Where(x => !string.IsNullOrEmpty(x))
        .Select(x => x.Split(' '))
        .Select(x => (direction: Enum.Parse<Direction>(x[0], true), amount: int.Parse(x[1])))
        .Aggregate((depth: 0, position: 0), (prev, current) => (
            depth: prev.depth + current.direction switch
            {
                Direction.up => -current.amount, // up means "less" depth
                Direction.down => current.amount, // down means "more" depth
                _ => 0
            },
            position: prev.position + current.direction switch
            {
                Direction.forward => current.amount,
                _ => 0
            }));

    Console.WriteLine($"End horizontal position of sub is {result.position}, with depth: {result.depth}.\n\nThese numbers multiplied are {result.depth * result.position}.");
    return result.depth * result.position;
}

int PrintDayTwoPartTwoAnswer(string inputFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 2 Part One using {inputFileName}.");
    Console.WriteLine("Dive!");

    var result = GetLines(inputFileName)
        // note because this must run over every element in the sequence and each result depends on the previous one,
        // it doesn't make much sense to do in parallel.  Runs in ~5ms even in debug mode on my machine as "regular" Linq,
        // so basically not needed anyway.
        .Where(x => !string.IsNullOrEmpty(x))
        .Select(x => x.Split(' '))
        .Select(x => (direction: Enum.Parse<Direction>(x[0], true), amount: int.Parse(x[1])))
        .Aggregate((depth: 0, position: 0, aim: 0), (prev, current) => (
            depth: prev.depth + current.direction switch
            {
                Direction.forward => prev.aim * current.amount,
                _ => 0
            },
            position: prev.position + current.direction switch
            {
                Direction.forward => current.amount,
                _ => 0
            },
            aim: prev.aim + current.direction switch
            {
                Direction.down => current.amount,
                Direction.up => -current.amount,
                _ => 0
            }));

    Console.WriteLine($"End horizontal position of sub is {result.position}, with depth: {result.depth}.\n\nThese numbers multiplied are {result.depth * result.position}.");
    return result.depth * result.position;
}

enum Direction
{
    up = 1,
    down = 2,
    forward = 3
}