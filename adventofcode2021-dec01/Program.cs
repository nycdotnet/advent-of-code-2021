
//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput1.txt";

//PrintDayOnePartOneAnswer(inputTextFileName);
PrintDayOnePartTwoAnswer(inputTextFileName);

void PrintDayOnePartOneAnswer(string inputFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 1 Part One using {inputFileName}.");
    var depthMeasurements = GetInputAsIntegerArray(inputFileName);
    var depthIncreases = NumberOfTimesDepthIncreases(depthMeasurements);
    Console.WriteLine($"The depth increases {depthIncreases} times.");
}

void PrintDayOnePartTwoAnswer(string inputFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 1 Part Two using {inputFileName}.");
    var depthMeasurements = GetInputAsIntegerArray(inputFileName);
    var depthIncreases = NumberOfTimesDepthIncreasesSlidingWindow(depthMeasurements, 3);
    Console.WriteLine($"The depth increases {depthIncreases} times.");
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

int[] GetInputAsIntegerArray(string fileName) => GetLines(fileName)
    .AsParallel()
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .Select(x => int.Parse(x))
    .ToArray();

string[] GetLines(string fileName) =>
    File.ReadAllText(Path.Combine(ProjectFolder(), fileName))
    .Split("\r\n");

string ProjectFolder() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));