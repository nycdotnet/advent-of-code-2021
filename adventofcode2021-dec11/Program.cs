using System.Diagnostics;
using static common.Utils;

Console.WriteLine("Day 11: Dumbo Octopus");

//var inputTextFileName = "example-input1.txt";
//var inputTextFileName = "example-input2.txt";
var inputTextFileName = "myPuzzleInput.txt";

var initialState = GetOctopusInitialState(inputTextFileName);

var part1SimulationStepCount = 100;
var part1FlashCount = SimulateOctopusFlashes(initialState, part1SimulationStepCount).Sum(x => x);
Console.WriteLine($"In part 1, simulating {part1SimulationStepCount} steps led to {part1FlashCount} flashes based on {inputTextFileName}.");
Debug.Assert(part1FlashCount == 1673);

var part2SimulationStepCount = 1000;
var part2FirstAllFlashStep = FirstStepWhenAllOctopiFlash(initialState, part2SimulationStepCount);
Console.WriteLine($"In part 2, while simulating up to {part2SimulationStepCount} steps, we found that all octopi flashed {(part2FirstAllFlashStep == -1 ? "never" : $"on step {part2FirstAllFlashStep}")} based on {inputTextFileName}.");
Debug.Assert(part2FirstAllFlashStep == 279);

IEnumerable<int> SimulateOctopusFlashes(short[][] initialOctopiState, int stepCount)
{
    var octopi = Octopus.Initialize(initialOctopiState);

    for (var step = 0; step < stepCount; step++)
    {
        yield return Octopus.Simulate(octopi);
    }
}

int FirstStepWhenAllOctopiFlash(short[][] initialOctopiState, int stepCount)
{
    var octopi = Octopus.Initialize(initialOctopiState);

    var necessaryFlashCount = initialOctopiState.Length * initialOctopiState[0].Length;

    for (var step = 0; step < stepCount; step++)
    {
        var flashCount = Octopus.Simulate(octopi);
        if (flashCount == necessaryFlashCount)
        {
            return step + 1;
        }
    }
    return -1;
}

short[][] GetOctopusInitialState(string file) => GetLines(file)
    .Select(line => line.ParseCharsToShorts().ToArray())
    .ToArray();

record Octopus
{
    public short Energy { get; private set; }
    public bool HasFlashed { get; private set; }

    public const int FlashIfEnergyGreaterThan = 9;

    public bool ShouldFlash() => Energy > FlashIfEnergyGreaterThan && !HasFlashed;

    public void Flash()
    {
        HasFlashed = true;
    }

    public void IncreaseEnergy(short amount)
    {
        if (!HasFlashed)
        {
            Energy += amount;
        }
    }

    public void ResetIfFlashed()
    {
        if (HasFlashed)
        {
            HasFlashed = false;
            Energy = 0;
        }
    }

    public static Octopus[][] Initialize(short[][] initialOctopiState)
    {
        var octopi = new Octopus[initialOctopiState.Length][];

        for (var y = 0; y < octopi.Length; y++)
        {
            octopi[y] = new Octopus[initialOctopiState[y].Length];
            for (var x = 0; x < initialOctopiState[y].Length; x++)
            {
                octopi[y][x] = new Octopus { Energy = initialOctopiState[y][x], HasFlashed = false };
            }
        }
        return octopi;
    }

    /// <summary>
    /// Returns the number of flashes on this step.
    /// </summary>
    public static int Simulate(Octopus[][] state)
    {
        var flashCount = 0;
        IncreaseAllEnergy(state, 1);

        int newFlashes;
        do
        {
            newFlashes = FlashAndIncrease(state);
            flashCount += newFlashes;
        } while (newFlashes > 0);

        ResetFlashed(state);

        return flashCount;
    }

    /// <summary>
    /// Looks for Octopi that are ready to flash.  Will flash them and increase adjacent
    /// Octopus energies.  Then returns the number that were newly flashed.
    /// </summary>
    private static int FlashAndIncrease(Octopus[][] state)
    {
        int flashCount = 0;
        for (var y = 0; y < state.Length; y++)
        {
            for (var x = 0; x < state[y].Length; x++)
            {
                var octopus = state[y][x];
                if (octopus.ShouldFlash())
                {
                    octopus.Flash();
                    IncreaseAdjacent(state, y, x);
                    flashCount++;
                }
            }
        }
        return flashCount;
    }

    private static void IncreaseAdjacent(Octopus[][] state, int y, int x)
    {
        if (y > 0)
        {
            if (x > 0)
            {
                // NW
                state[y-1][x-1].IncreaseEnergy(1);
            }
            // N
            state[y-1][x].IncreaseEnergy(1);
            if (x + 1 < state[y - 1].Length)
            {
                // NE
                state[y - 1][x + 1].IncreaseEnergy(1);
            }
        }
        if (x > 0)
        {
            // W
            state[y][x - 1].IncreaseEnergy(1);
        }
        if (x + 1 < state[y].Length)
        {
            // E
            state[y][x + 1].IncreaseEnergy(1);
        }
        if (y + 1 < state.Length)
        {
            if (x > 0)
            {
                // SW
                state[y + 1][x - 1].IncreaseEnergy(1);
            }
            // S
            state[y + 1][x].IncreaseEnergy(1);
            if (x + 1 < state[y + 1].Length)
            {
                // SE
                state[y + 1][x + 1].IncreaseEnergy(1);
            }
        }
    }

    public static void IncreaseAllEnergy(Octopus[][] state, short amount)
    {
        for (var y = 0; y < state.Length; y++)
        {
            for (var x = 0; x < state[y].Length; x++)
            {
                state[y][x].IncreaseEnergy(amount);
            }
        }
    }

    public static void ResetFlashed(Octopus[][] state)
    {
        for (var y = 0; y < state.Length; y++)
        {
            for (var x = 0; x < state[y].Length; x++)
            {
                state[y][x].ResetIfFlashed();
            }
        }
    }
}
