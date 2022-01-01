using System.Diagnostics;
using static common.Utils;

Console.WriteLine("Day 11: Passage Pathing");

//var inputTextFileName = "example-input1.txt";
//var inputTextFileName = "example-input2.txt";
//var inputTextFileName = "example-input3.txt";
var inputTextFileName = "myPuzzleInput.txt";

var answer1 = GetPathsThroughSmallCavesAtMostOnce(inputTextFileName);
Debug.Assert(answer1 == 4378);

var answer2 = GetPathsWithBonusSmallCave(inputTextFileName);
Debug.Assert(answer2 == 133621);

int GetPathsThroughSmallCavesAtMostOnce(string file)
{
    var allConnections = GetLines(file).Select(l => l.Split('-')).Select(x => new Connection { Start = x[0], End = x[1] }).ToArray();
    allConnections = allConnections
        .Concat(allConnections.Select(x => new Connection { End = x.Start, Start = x.End }))
        .Where(x => x.Start != "end")
        .Where(x => x.End != "start")
        .ToArray();

    var connectionsByStart = allConnections.ToLookup(x => x.Start);

    var allValidPaths = FindPaths(new Journey { Path = new List<string> { "start" } });

    var pathString = string.Join('\n', allValidPaths.Select(x => string.Join(",", x)).OrderBy(x => x, StringComparer.Ordinal));

    Console.WriteLine($"Found {allValidPaths.Count} paths for part 1 using input file {file}.");

    return allValidPaths.Count;

    List<List<string>> FindPaths(Journey candidateJourney)
    {
        var current = candidateJourney.Path.Last();
        var result = new List<List<string>>();
        var connections = connectionsByStart[current];
        foreach (var c in connections)
        {
            if (c.LeadsToEnd)
            {
                result.Add(candidateJourney.Path.Concat(new string[] { c.End }).ToList());
            }
            else if (c.LeadsToSmallCave)
            {
                if (!candidateJourney.Path.Contains(c.End))
                {
                    var newPaths = FindPaths(candidateJourney with { Path = candidateJourney.Path.Concat(new string[] { c.End }).ToList() });
                    result.AddRange(newPaths);
                }
            }
            else if (!c.LeadsToSmallCave)
            {
                var newPaths = FindPaths(candidateJourney with { Path = candidateJourney.Path.Concat(new string[] { c.End }).ToList() });
                result.AddRange(newPaths);
            }
        }
        return result;
    }
}

int GetPathsWithBonusSmallCave(string file)
{
    var allConnections = GetLines(file).Select(l => l.Split('-')).Select(x => new Connection { Start = x[0], End = x[1] }).ToArray();
    allConnections = allConnections
        .Concat(allConnections.Select(x => new Connection { End = x.Start, Start = x.End }))
        .Where(x => x.Start != "end")
        .Where(x => x.End != "start")
        .ToArray();

    var connectionsByStart = allConnections.ToLookup(x => x.Start);

    var allValidPaths = FindPaths(new Journey { Path = new List<string> { "start" } });

    var pathString = string.Join('\n', allValidPaths.Select(x => string.Join(",", x)).OrderBy(x => x, StringComparer.Ordinal));

    Console.WriteLine($"Found {allValidPaths.Count} paths for part 2 using input file {file}.");

    return allValidPaths.Count;

    List<List<string>> FindPaths(Journey candidateJourney)
    {
        var current = candidateJourney.Path.Last();
        var result = new List<List<string>>();
        var connections = connectionsByStart[current];
        foreach (var c in connections)
        {
            if (c.LeadsToEnd)
            {
                result.Add(candidateJourney.Path.Concat(new string[] { c.End }).ToList());
            }
            else if (c.LeadsToSmallCave)
            {
                var alreadyVisited = candidateJourney.Path.Contains(c.End);
                var shouldAssignToBonus = alreadyVisited && string.IsNullOrEmpty(candidateJourney.BonusSmallCave);
                if (!alreadyVisited || shouldAssignToBonus)
                {
                    var newPaths = FindPaths(candidateJourney with
                    {
                        Path = candidateJourney.Path.Concat(new string[] { c.End }).ToList(),
                        BonusSmallCave = !string.IsNullOrEmpty(candidateJourney.BonusSmallCave) ? candidateJourney.BonusSmallCave :
                            shouldAssignToBonus ? c.End :
                            ""
                    });
                    result.AddRange(newPaths);
                }
            }
            else if (!c.LeadsToSmallCave)
            {
                var newPaths = FindPaths(candidateJourney with { Path = candidateJourney.Path.Concat(new string[] { c.End }).ToList() });
                result.AddRange(newPaths);
            }
        }
        return result;
    }
}

record Journey
{
    public List<string> Path { get; set; }
    public string BonusSmallCave { get; set; } = "";
}

record Connection
{
    public string Start { get; set; }
    public string End { get; set; }
    public bool LeadsToSmallCave => End.All(c => char.IsLower(c));
    public bool LeadsToEnd => End == "end";
}