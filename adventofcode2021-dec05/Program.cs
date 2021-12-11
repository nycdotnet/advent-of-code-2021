using System.Diagnostics;
using System.Text.RegularExpressions;
using static common.Utils;


//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var part1Answer = PrintDay5Answer(inputTextFileName, "One", ls => ls.x1 == ls.x2 || ls.y1 == ls.y2);
Debug.Assert(part1Answer == 4873);

var part2Answer = PrintDay5Answer(inputTextFileName, "Two", _ => true);
Debug.Assert(part2Answer == 19472);


int PrintDay5Answer(string inputFileName, string partDescription, Func<LineSegment, bool> predicate)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 5 Part {partDescription} using {inputTextFileName}.");
    Console.WriteLine("Hydrothermal Venture");

    var textLines = GetLines(inputFileName);
    var lineSegments = textLines
        .Select(ls => LineSegment.Parse(ls))
        .Where(predicate)
        .ToArray();

    var (maxX, maxY) = lineSegments.Aggregate(
        (maxX: -1, maxY: -1),
        (acc, next) => (
            maxX: Math.Max(Math.Max(acc.maxX, next.x1), next.x2),
            maxY: Math.Max(Math.Max(acc.maxY, next.y1), next.y2)));

    var seaFloorMap = new int[maxX + 1, maxY + 1];

    foreach (var ls in lineSegments)
    {
        foreach (var coordinate in ls.Rasterize())
        {
            seaFloorMap[coordinate.x, coordinate.y] += 1;
        }
    }

    //DrawSeaFloor(seaFloorMap);

    var moreThanOneCount = 0;
    for (var x = 0; x <= seaFloorMap.GetUpperBound(0); x++)
    {
        for (var y = 0; y <= seaFloorMap.GetUpperBound(1); y++)
        {
            if (seaFloorMap[x, y] > 1)
            {
                moreThanOneCount++;
            }
        }
    }

    Console.WriteLine($"The number of points where at least two lines overlap is {moreThanOneCount}.");

    return moreThanOneCount;
}

void DrawSeaFloor(int[,] data)
{
    for (var y = 0; y <= data.GetUpperBound(0); y++)
    {
        var printLine = string.Create(data.GetUpperBound(1) + 1, new char[data.GetUpperBound(0) + 1], (Span<char> result, char[] _) => {
            for (var x = 0; x < data.GetUpperBound(0) + 1; x++)
            {
                result[x] = data[x, y] switch
                {
                    0 => '.',
                    < 10 => (char)(data[x, y] + '0'),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        });
        Console.WriteLine(printLine);
    }
}

record LineSegment
{
    public int x1 { get; init; }
    public int y1 { get; init; }
    public int x2 { get; init; }
    public int y2 { get; init; }

    public static LineSegment Parse(string lineSegment)
    {
        var x = LineSegmentParseRegex.Match(lineSegment);
        return new LineSegment
        {
            x1 = int.Parse(x.Groups[nameof(x1)].ValueSpan),
            y1 = int.Parse(x.Groups[nameof(y1)].ValueSpan),
            x2 = int.Parse(x.Groups[nameof(x2)].ValueSpan),
            y2 = int.Parse(x.Groups[nameof(y2)].ValueSpan),
        };
    }

    public IEnumerable<(int x, int y)> Rasterize()
    {
        // first we normalize so that the line always points toward the east,
        // south, or south east/north east.

        var xSame = x1 == x2;
        var ySame = y1 == y2;
        int min, max;
        switch (xSame, ySame)
        {
            case (true, true):
                yield return (x1, y1);
                break;
            case (false, true):
                min = Math.Min(x1, x2);
                max = Math.Max(x1, x2);
                for (var x = min; x <= max; x++)
                {
                    yield return (x, y1);
                }
                break;
            case (true, false):
                min = Math.Min(y1, y2);
                max = Math.Max(y1, y2);
                for (var y = min; y <= max; y++)
                {
                    yield return (x1, y);
                }
                break;
            case (false, false):

                // NE is fine SE is fine
                // 1,0 -> 0,1 SW
                // 1,1 -> 0,0 NW
                // 0,1 => 1,0 NE OK
                // 0,0 => 1,1 SE OK
                var (_x1, _y1, _x2, _y2) = (x1 > x2) switch
                {
                    true => (x2, y2, x1, y1),
                    _ => (x1, y1, x2, y2)
                };
                var yOffset = _y1 < _y2 ? 1 : -1;

                for (int x = _x1, y = _y1; x <= _x2; x++, y += yOffset)
                {
                    yield return (x, y);
                }
                break;
        }
    }

    private static readonly Regex LineSegmentParseRegex =
        new(@"(?<x1>\d*),(?<y1>\d*) -> (?<x2>\d*),(?<y2>\d*)", RegexOptions.Singleline | RegexOptions.Singleline);
}