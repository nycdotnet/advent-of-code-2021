using System.Diagnostics;
using static common.Utils;

Console.WriteLine("Day 9: Smoke Basin");

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var heightMap = GetDepthMap(inputTextFileName);

var answer1 = FindSumOfLowPointRiskLevels(inputTextFileName, heightMap);
Debug.Assert(answer1 == 603);

var answer2 = FindBasinInfo(inputTextFileName, heightMap);
Debug.Assert(answer2 == 786780);


short[][] GetDepthMap(string file) => GetLines(file)
        .Select(line => line.ParseCharsToShorts().ToArray())
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

int FindBasinInfo(string file, short[][] heightMap)
{
    var basinIds = new short?[heightMap.Length][];
    for (var y = 0; y < heightMap.Length; y++)
    {
        basinIds[y] = new short?[heightMap[y].Length];
    }

    // NOTE: null is undecided, -1 is no basin.

    short nextBasinId = 0;

    for (var y = 0; y < heightMap.Length; y++)
    {
        for (var x = 0; x < heightMap[y].Length; x++)
        {
            if (heightMap[y][x] == 9)
            {
                basinIds[y][x] = -1;
                continue;
            }
            short basinId;
            if (TryGetBasinLeft(basinIds, x, y, out basinId))
            {
                basinIds[y][x] = basinId;
            }
            else if (TryGetBasinUp(basinIds, x, y, out basinId))
            {
                basinIds[y][x] = basinId;
            }
            else
            {
                basinIds[y][x] = nextBasinId;
                nextBasinId++;
            }
        }
    }

    // consolidate the basins
    for (var y = 0; y < basinIds.Length; y++)
    {
        for (var x = 0; x < basinIds[y].Length; x++)
        {
            var currentBasinId = basinIds[y][x]!.Value;
            if (currentBasinId == -1)
            {
                continue;
            }

            if (TryGetBasinUp(basinIds, x, y, out var otherBasinId) && currentBasinId != otherBasinId)
            {
                ReplaceAll(basinIds, currentBasinId, otherBasinId);
            }
        }
    }

    var basinSizes = new Dictionary<short, short>();

    for (var y = 0; y < basinIds.Length; y++)
    {
        for (var x = 0; x < basinIds[y].Length; x++)
        {
            var basinId = basinIds[y][x]!.Value;
            if (!basinSizes.TryAdd(basinId, 1))
            {
                basinSizes[basinId] += 1;
            }
        }
    }

    var result = basinSizes
        .Select(kvp => kvp)
        .Where(x => x.Key != -1)
        .OrderByDescending(x => x.Value)
        .Take(3)
        .Aggregate(1, (p, n) => p * n.Value);

    Console.WriteLine($"Part two answer - the size of the three largest basins multiplied together is {result}.");

    // draw the basins (really only useful with the example data because the 
    //for (var y = 0; y < basinIds.Length; y++)
    //{
    //    Console.WriteLine(string.Join("",
    //        basinIds[y].Select(s => s.HasValue ? s.Value == -1 ? "X" : s.Value.ToString() : ".")));
    //}

    return result;
}

static void ReplaceAll(short?[][] array, short findValue, short replaceValue)
{
    for (var y = 0; y < array.Length;y++)
    {
        for (var x = 0; x < array[y].Length; x++)
        {
            if (array[y][x] == findValue)
            {
                array[y][x] = replaceValue;
            }
        }
    }
}

static short RiskLevel(short[][] heightMap, int x, int y) => (short)(heightMap[y][x] + 1);

static bool IsLowPoint(short[][] heightMap, int x, int y) =>
    IsLowerThanLeft(heightMap, x, y) &&
    IsLowerThanRight(heightMap, x, y) &&
    IsLowerThanUp(heightMap, x, y) &&
    IsLowerThanDown(heightMap, x, y);

static bool TryGetBasinLeft(short?[][] basinIds, int x, int y, out short basinId)
{
    if (x == 0)
    {
        basinId = -1;
        return false;
    }
    basinId = basinIds[y][x - 1] ?? -1;
    return basinId != -1;
}

static bool TryGetBasinUp(short?[][] basinIds, int x, int y, out short basinId)
{
    if (y == 0)
    {
        basinId = -1;
        return false;
    }
    basinId = basinIds[y - 1][x] ?? -1;
    return basinId != -1;
}

static bool IsLowerThanLeft(short[][] heightMap, int x, int y) =>
    x == 0 || heightMap[y][x] < heightMap[y][x - 1];
static bool IsLowerThanRight(short[][] heightMap, int x, int y) =>
    x == heightMap[y].Length - 1 || heightMap[y][x] < heightMap[y][x + 1];
static bool IsLowerThanUp(short[][] heightMap, int x, int y) =>
    y == 0 || heightMap[y][x] < heightMap[y - 1][x];
static bool IsLowerThanDown(short[][] heightMap, int x, int y) =>
    y == heightMap.Length - 1 || heightMap[y][x] < heightMap[y + 1][x];
