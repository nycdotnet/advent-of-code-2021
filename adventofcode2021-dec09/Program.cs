using System.Diagnostics;
using static common.Utils;

Console.WriteLine("Day 9: Smoke Basin");

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var heightMap = GetDepthMap(inputTextFileName);

var answer1 = FindSumOfLowPointRiskLevels(inputTextFileName, heightMap);
Debug.Assert(answer1 == 603);



short[][] GetDepthMap(string file) => GetLines(file)
        .Select(line => Helpers.ParseCharsToShorts(line).ToArray())
        .ToArray();

int FindSumOfLowPointRiskLevels(string file, short[][] heightMap)
{
    var result = 0;
    for (var y = 0; y < heightMap.Length; y++)
    {
        for (var x = 0; x < heightMap[y].Length; x++)
        {
            if (IsLowPoint(heightMap, x, y))
            {
                result += RiskLevel(heightMap, x, y);
            }
        }
    }

    Console.WriteLine($"Part one answer - the risk level sum is {result}.");

    return result;
}

int FindBasins(string file, short[][] heightMap)
{
    throw new NotImplementedException();
}


static short RiskLevel(short[][] heightMap, int x, int y) => (short)(heightMap[y][x] + 1);

static bool IsLowPoint(short[][] heightMap, int x, int y) {
    var thisPoint = heightMap[y][x];
    return IsLowerThanLeft() && IsLowerThanRight() && IsLowerThanUp() && IsLowerThanDown();

    bool IsLowerThanLeft() => x == 0 || thisPoint < heightMap[y][x - 1];
    bool IsLowerThanRight() => x == heightMap[y].Length -1 || thisPoint < heightMap[y][x + 1];
    bool IsLowerThanUp() => y == 0 || thisPoint < heightMap[y - 1][x];
    bool IsLowerThanDown() => y == heightMap.Length - 1 || thisPoint < heightMap[y + 1][x];
}

public static class Helpers
{
    public static short[] ParseCharsToShorts(string str)
    {
        var ss = str.AsSpan();
        var result = new short[str.Length];
        for (var i = 0; i < ss.Length; i++)
        {
            result[i] = short.Parse(ss.Slice(i,1));
        }
        return result;
    }
}