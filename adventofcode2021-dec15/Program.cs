using common;
using System.Diagnostics;
using static common.Utils;

Console.WriteLine("Day 15: Chiton");

var inputTextFileName = "example-input1.txt";
//var inputTextFileName = "myPuzzleInput.txt";

var map = GetMap(inputTextFileName);

var answer1 = DoPart1(map);
Debug.Assert(answer1 == 3406);

int DoPart1(short[][] map)
{
    // theory: don't ever re-enter a previous spot.

    var initialPosition = new Point2d { X = 0, Y = 0 };

    var acrossDownJourney = new Journey(initialPosition);
    while (acrossDownJourney.CanMoveRight(map))
    {
        acrossDownJourney.MoveRight(map);
    }
    while (acrossDownJourney.CanMoveDown(map))
    {
        acrossDownJourney.MoveDown(map);
    }
    Debug.Assert(acrossDownJourney.IsComplete(map));

    var downAcrossJourney = new Journey(initialPosition);
    while (downAcrossJourney.CanMoveDown(map))
    {
        downAcrossJourney.MoveDown(map);
    }
    while (downAcrossJourney.CanMoveRight(map))
    {
        downAcrossJourney.MoveRight(map);
    }
    Debug.Assert(downAcrossJourney.IsComplete(map));

    var initialBestRisk = Math.Min(acrossDownJourney.AccumulatedRisk, downAcrossJourney.AccumulatedRisk);


    var minimumRisk = Explore(map, new Journey(), initialBestRisk);




    return 0;
}

Journey Explore(short[][] map, Journey journey, int initialBestRisk)
{
    var newBestRisk = initialBestRisk;
    var bestJourney = journey;
    if (journey.CanMoveRight(map) && !journey.HasVisitedRight)
    {
        //var rightJourney = bestJourney with { };
        //rightJourney.MoveRight(map);
        //var rightJourney = Explore(map, journey, newBestRisk);
        //if (rightJourney.AccumulatedRisk < newBestRisk)
        //{
        //    newBestRisk = rightJourney.AccumulatedRisk;
        //    bestJourney = rightJourney;
        //}
    }

    if (journey.CanMoveDown(map) && !journey.HasVisitedDown)
    {
        //var downJourney = Explore(map, journey, newBestRisk);
        //if (downJourney.AccumulatedRisk < newBestRisk)
        //{
        //    newBestRisk = downJourney.AccumulatedRisk;
        //    bestJourney = downJourney;
        //}
    }


    return bestJourney;
}

short[][] GetMap(string file) => GetLines(file)
    .Select(line => line.ParseCharsToShorts().ToArray())
    .ToArray();

public record Journey
{
    public HashSet<Point2d> Path { get; set; }
    public Point2d CurrentPosition { get; set; }
    public int AccumulatedRisk { get; private set; }

    public Journey() {
        Path = new();
    }
    public Journey(Point2d initial)
    {
        Path = new() { initial };
        CurrentPosition = initial;
    }

    public bool IsComplete(short[][] map) => CurrentPosition.Y == map.Length - 1 && CurrentPosition.X == map[0].Length - 1;


    public bool CanMoveRight(short[][] map) => CurrentPosition.X + 1 < map[CurrentPosition.Y].Length;
    public bool CanMoveLeft(short[][] _) => CurrentPosition.X > 0;
    public bool CanMoveDown(short[][] map) => CurrentPosition.Y + 1 < map.Length;
    public bool CanMoveUp(short[][] _) => CurrentPosition.Y > 0;

    public bool HasVisitedRight => Path.Contains(CurrentPosition with { X = CurrentPosition.X + 1 });
    public bool HasVisitedLeft => Path.Contains(CurrentPosition with { X = CurrentPosition.X - 1 });
    public bool HasVisitedDown => Path.Contains(CurrentPosition with { X = CurrentPosition.Y + 1 });
    public bool HasVisitedUp => Path.Contains(CurrentPosition with { X = CurrentPosition.Y - 1 });

    /// <summary>
    /// Moves right and returns the new risk level.  Throws if not possible.
    /// </summary>
    public short MoveRight(short[][] map)
    {
        CurrentPosition = CurrentPosition with { X = CurrentPosition.X + 1 };
        var newRisk = map[CurrentPosition.Y][CurrentPosition.X];
        AccumulatedRisk += newRisk;
        return newRisk;
    }

    /// <summary>
    /// Moves left and returns the new risk level.  Throws if not possible.
    /// </summary>
    public short MoveLeft(short[][] map)
    {
        CurrentPosition = CurrentPosition with { X = CurrentPosition.X - 1 };
        var newRisk = map[CurrentPosition.Y][CurrentPosition.X];
        AccumulatedRisk += newRisk;
        return newRisk;
    }

    /// <summary>
    /// Moves down and returns the new risk level.  Throws if not possible.
    /// </summary>
    public short MoveDown(short[][] map)
    {
        CurrentPosition = CurrentPosition with { Y = CurrentPosition.Y + 1 };
        var newRisk = map[CurrentPosition.Y][CurrentPosition.X];
        AccumulatedRisk += newRisk;
        return newRisk;
    }

    /// <summary>
    /// Moves up and returns the new risk level.  Throws if not possible.
    /// </summary>
    public short MoveUp(short[][] map)
    {
        CurrentPosition = CurrentPosition with { Y = CurrentPosition.Y - 1 };
        var newRisk = map[CurrentPosition.Y][CurrentPosition.X];
        AccumulatedRisk += newRisk;
        return newRisk;
    }
}
