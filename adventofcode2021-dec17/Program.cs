using System.Diagnostics;
using System.Text.RegularExpressions;
using static common.Utils;

var targetAreaTextExample = "target area: x=20..30, y=-10..-5";
var targetAreaText = GetLines("myPuzzleInput.txt").First();

var ta = TargetArea.Parse(targetAreaText);

// we will assume X and Y must be positive for part 1.

//DoPart1(ta);

DoPart2(ta);

// for Part 2: first guess was 1802 and this was too low.


void DoPart2(TargetArea ta)
{
	var hits = new List<HitRecord>();

	if (ta.MinX <= 0)
	{
		throw new NotSupportedException("Only supports shooting up down or right.");
	}

    for (var x = 0; x <= ta.MaxX; x++)
    {
        Console.WriteLine($"Simulating X velocity: {x}.");
        for (var y = ta.MinY; y < 1000; y++)
        {
            var hit = Simulate(x, y, ta);
            if (hit.HasValue)
            {
                hits.Add(new HitRecord { Height = hit.Value, Xv = x, Yv = y });
            }
        }
    }
	Console.WriteLine($"Found {hits.Count} distinct hits");
}


void DoPart1(TargetArea ta)
{
    var best = new HitRecord();

    for (var x = 0; x < 200; x++)
    {
        var bestText = best.Height > int.MinValue ? $"Current best height is {best.Height} when launching at Xv:{best.Xv} Yv:{best.Yv}" : "No hits detected yet.";
        Console.WriteLine($"Simulating X velocity: {x}.  {bestText}");
        for (var y = 0; y < 1000; y++)
        {
            var hit = Simulate(x, y, ta);
            if (hit.HasValue && hit.Value > best.Height)
            {
                best.Height = hit.Value;
                best.Xv = x;
                best.Yv = y;
            }
        }
    }

    Console.WriteLine($"Best known height is {best.Height} when launching at Xv:{best.Xv} Yv:{best.Yv}");
}


int? Simulate(int VelocityX, int VelocityY, TargetArea ta)
{
    var probe = new Probe();
	int maxHeight = 0;
	//Console.WriteLine($"Simulating X:{VelocityX},Y:{VelocityY}.");

    probe.Fire(VelocityX, VelocityY);
    for (var i = 0; i < 3000; i++) // the limit here is just a sanity check
    {
        Debug.Assert(i < 2999);
        probe.Step();
        var isHit = ta.IsHit(probe.PositionX, probe.PositionY);
        var couldHit = isHit || ta.CouldHit(probe.PositionX, probe.PositionY, probe.VelocityX, probe.VelocityY);
        if (!couldHit)
        {
            return null;
        }
        if (probe.PositionY > maxHeight)
		{
			maxHeight = probe.PositionY;
		}
        //Console.WriteLine($"Probe is now at {probe.PositionX},{probe.PositionY}.  This is {(isHit ? "" : "NOT ")}a hit. (press any key to continue)");
        if (isHit)
        {
			return maxHeight;
        }
    }
	throw new ApplicationException("too many steps.");
}

struct HitRecord
{
    public HitRecord()
    {
        Xv = 0;
        Yv = 0;
        Height = int.MinValue;
    }
    public int Xv;
    public int Yv;
    public int Height;
}


public class TargetArea
{
	public static TargetArea Parse(string content)
	{
		var regex = new Regex("target area: x=(?<MinX>-?\\d*)\\.\\.(?<MaxX>-?\\d*), y=(?<MinY>-?\\d*)\\.\\.(?<MaxY>-?\\d*)");
		var match = regex.Match(content);
		return new TargetArea {
			MinX = int.Parse(match.Groups["MinX"].Value),
            MaxX = int.Parse(match.Groups["MaxX"].Value),
            MinY = int.Parse(match.Groups["MinY"].Value),
            MaxY = int.Parse(match.Groups["MaxY"].Value)
        };
    }

	public bool IsHit(int X, int Y) => X >= MinX && X <= MaxX && Y >= MinY && Y <= MaxY;
	public bool CouldHit(int X, int Y, int XVelocity, int YVelocity)
	{
		if (XVelocity == 0)
		{
			if (IsWithinXBoundary())
			{
				return !IsBelowTheBottomOfTheTarget();
			}
			return false;
		}

		// it is traveling horizontally
		if (IsBelowTheBottomOfTheTarget() && !IsClimbing())
		{
			return false;
		}
        if (IsTravelingEast())
		{
			return X <= MaxX;
		}
		// it's traveling west.
		return X <= MinX;
        bool IsBelowTheBottomOfTheTarget() => Y < MinY;
		bool IsTravelingEast() => XVelocity > 0;
		bool IsClimbing() => YVelocity > 0;
		bool IsWithinXBoundary() => X >= MinX && X <= MaxX;
	}
	

	public int MinX { get; init; }
	public int MaxX { get; init; }
	public int MinY { get; init; }
	public int MaxY { get; init; }
}

public class Probe
{
	public Probe()
	{
		PositionX = 0;
		PositionY = 0;
	}

	public void Fire(int VelocityX, int VelocityY)
	{
		this.VelocityX = VelocityX;
		this.VelocityY = VelocityY;
	}

	public void Step()
	{
		PositionX += VelocityX;
		PositionY += VelocityY;

		// drag
		if (VelocityX > 0)
		{
			VelocityX -= 1;
		}
		else if (VelocityX < 0)
		{
			VelocityX += 1;
		}

		// gravity
		VelocityY -= 1;
	}

	public int PositionX { get; private set; }
	public int PositionY { get; private set; }
	public int VelocityX { get; private set; }
	public int VelocityY { get; private set; }
}