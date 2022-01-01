using System.Diagnostics;
using static common.Utils;

Console.WriteLine("Day 13: Transparent Origami");

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var answer1 = DotsVisibleAfterFirstFold(inputTextFileName);
Debug.Assert(answer1 == 682);

FoldAndRender(inputTextFileName);

int DotsVisibleAfterFirstFold(string file)
{
    var lines = GetLines(file);
    var origami = DotsOrigami.Parse(lines);
    origami.FoldOnce();
    return origami.Points.Count;
}

void FoldAndRender(string file)
{
    var lines = GetLines(file);
    var origami = DotsOrigami.Parse(lines);
    
    while(origami.Folds.Any())
    {
        origami.FoldOnce();
        origami.Folds.RemoveAt(0);
    }
    origami.Render();
}

public class DotsOrigami
{
    public List<Point> Points { get; set; }
    public List<Fold> Folds { get; set; }

    public void FoldOnce()
    {
        var foldInfo = Folds.First();
        foreach (var point in Points)
        {
            point.Fold(foldInfo);
        }
        Consolidate();
    }

    public void Consolidate()
    {
        Points = Points.Distinct().ToList();
    }

    public static DotsOrigami Parse(IEnumerable<string> input)
    {
        var result = new DotsOrigami()
        {
            Points = new(),
            Folds = new()
        };

        foreach (var line in input)
        {
            if (line.StartsWith("fold along"))
            {
                var items = line.Split('=');
                result.Folds.Add(new() { Axis = items[0][^1], Position = int.Parse(items[1]) });
            }
            else
            {
                var items = line.Split(',');
                result.Points.Add(new() { X = int.Parse(items[0]), Y = int.Parse(items[1]) });
            }
        }
        return result;
    }

    public void Render()
    {
        // there's probably some optimization to be done in here, but this is good enough for our input size.

        var (maxX, maxY) = Points.Aggregate((maxX: 0, maxY: 0),
            (acc, next) => (maxX: Math.Max(acc.maxX, next.X), maxY: Math.Max(acc.maxY, next.Y)));

        var pointsByY = Points.ToLookup(p => p.Y);

        for (var y = 0; y < maxY + 1; y++)
        {
            var output = new char[maxX + 1];
            Array.Fill(output, '.');
            foreach (var point in pointsByY[y])
            {
                output[point.X] = '#';
            }
            Console.WriteLine(new string(output));
        }
    }
}

public record Point
{
    public int X { get; set; }
    public int Y { get; set; }

    internal void Fold(Fold foldInfo)
    {
        if (foldInfo.Axis == 'y')
        {
            if (Y > foldInfo.Position)
            {
                Y = foldInfo.Position - (Y - foldInfo.Position);
            }
        }
        else if (foldInfo.Axis == 'x')
        {
            if (X > foldInfo.Position)
            {
                X = foldInfo.Position - (X - foldInfo.Position);
            }
        }
        else
        {
            throw new NotSupportedException();
        }
    }
}

public record Fold
{
    public char Axis { get; set; }
    public int Position { get; set; }
}