//using adventofcode2021_dec15;
using AStar;
using common;
using static common.Utils;

Console.WriteLine("Day 15: Chiton");

// todo: in real world, assert that map is not jagged.
var exampleMap1 = GetMap("example-input1.txt");
var myMap1 = GetMap("myPuzzleInput.txt");

{ 
    Console.WriteLine($"Starting example input at {DateTime.Now}:");
    var aStarExample = new Dec15Grid(exampleMap1, (0, 0), (9, 9));
    Console.WriteLine($"Map is {exampleMap1.Length} tall by {exampleMap1.FirstOrDefault()?.Length ?? 0} wide");
    Console.WriteLine($"Example input minimum risk: {aStarExample.OptimalPathCost} from {aStarExample.Start} to {aStarExample.Goal}");
}
{ 
    Console.WriteLine($"Starting my input 1 at {DateTime.Now}:");
    var aStarMap1 = new Dec15Grid(myMap1, (0, 0), (99, 99));
    Console.WriteLine($"Map is {myMap1.Length} tall by {myMap1.FirstOrDefault()?.Length ?? 0} wide");
    Console.WriteLine($"My input minimum risk: {aStarMap1.OptimalPathCost} from {aStarMap1.Start} to {aStarMap1.Goal}");
}
{
    Console.WriteLine($"Starting example input 2 at {DateTime.Now}:");

    var exampleMap2 = ExpandMap(exampleMap1, 5);
    var aStarExampleMap2 = new Dec15Grid(exampleMap2, (0, 0), (49, 49));

    Console.WriteLine($"Map is {exampleMap2.Length} tall by {exampleMap2.FirstOrDefault()?.Length ?? 0} wide");
    Console.WriteLine($"Example input minimum risk: {aStarExampleMap2.OptimalPathCost} from {aStarExampleMap2.Start} to {aStarExampleMap2.Goal}");
}
{
    Console.WriteLine($"Starting my input 2 at {DateTime.Now}:");

    var myMap2 = ExpandMap(myMap1, 5);
    var aStarMyMap2 = new Dec15Grid(myMap2, (0, 0), ((myMap2.FirstOrDefault()?.Length ?? 0) - 1, myMap2.Length - 1));

    Console.WriteLine($"Map is {myMap2.Length} tall by {myMap2.FirstOrDefault()?.Length ?? 0} wide");
    Console.WriteLine($"My input (expanded) minimum risk: {aStarMyMap2.OptimalPathCost} from {aStarMyMap2.Start} to {aStarMyMap2.Goal}");
}




// Copies the map as "tiles" and applies a risk adjustment.
static short[][] ExpandMap(short[][] inputMap, int expansionRatio)
{
    var result = new short[inputMap.Length * expansionRatio][];

    for (var y = 0; y < inputMap.Length; y++)
    {
        for (var yTileIndex = 0; yTileIndex < expansionRatio; yTileIndex++)
        {
            var yOffset = y + (yTileIndex * inputMap.Length);
            result[yOffset] = new short[inputMap[y].Length * expansionRatio];

            for (var x = 0; x < inputMap[y].Length; x++)
            {
                for (var xTileIndex = 0; xTileIndex < expansionRatio; xTileIndex++)
                {
                    var xOffset = x + (xTileIndex * inputMap[y].Length);

                    short risk = (short)(inputMap[y][x] + yTileIndex + xTileIndex);
                    while (risk > 9)
                    {
                        risk -= 9;
                    }

                    result[yOffset][xOffset] = risk;
                }
            }
        }
    }
    return result;
}

short[][] GetMap(string file) => GetLines(file)
    .Select(line => line.ParseCharsToShorts().ToArray())
    .ToArray();

