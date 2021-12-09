using System.Diagnostics;
using static common.Utils;

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";

var part1Answer = PrintDay4PartOneAnswer(inputTextFileName);
Debug.Assert(part1Answer == 64084);

var part2Answer = PrintDay4PartTwoAnswer(inputTextFileName);
Debug.Assert(part2Answer == 12833);

int PrintDay4PartOneAnswer(string inputTextFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 4 Part One using {inputTextFileName}.");
    Console.WriteLine("Giant Squid");

    var bingo = GetBingoSetup(inputTextFileName);
    var winners = FindEarliestWinningBoards(bingo.numbers, bingo.boards);

    var winningBoard = winners.winningBoards.Single();
    var sumUnmarked = winningBoard.SumOfUnmarkedNumbers();
    var product = sumUnmarked * winners.lastNumberCalled;

    Console.WriteLine($"The last number called was {winners.lastNumberCalled}.");
    Console.WriteLine($"The sum of unmarked numbers on the winning board is {sumUnmarked}.");
    Console.WriteLine($"The product is {product}.");

    return product;
}

int PrintDay4PartTwoAnswer(string inputTextFileName)
{
    Console.WriteLine($"Solving Advent Of Code 2021 Day 4 Part Two using {inputTextFileName}.");
    Console.WriteLine("Giant Squid");

    var bingo = GetBingoSetup(inputTextFileName);
    var winners = FindLastWinningBoards(bingo.numbers, bingo.boards);

    var winningBoard = winners.winningBoards.Single();
    var sumUnmarked = winningBoard.SumOfUnmarkedNumbers();
    var product = sumUnmarked * winners.lastNumberCalled;

    Console.WriteLine($"The last number called was {winners.lastNumberCalled}.");
    Console.WriteLine($"The sum of unmarked numbers on the LAST winning board is {sumUnmarked}.");
    Console.WriteLine($"The product is {product}.");

    return product;
}

(int[] numbers, List<BingoBoard> boards) GetBingoSetup(string inputFileName)
{
    var lines = GetLines(inputFileName);

    var numbers = lines[0].Split(',').Select(x => int.Parse(x)).ToArray();

    var boards = new List<BingoBoard>();

    int lineIndex = 2;
    while (true)
    {
        if (lineIndex + BingoBoard.Scale > lines.Length)
        {
            break;
        }
        boards.Add(BingoBoard.Parse(lines.AsSpan().Slice(lineIndex, BingoBoard.Scale)));
        lineIndex += BingoBoard.Scale + 1;
    }

    return (numbers, boards);
}

(IEnumerable<BingoBoard> winningBoards, int lastNumberCalled) FindEarliestWinningBoards(IEnumerable<int> numbers, List<BingoBoard> boards)
{
    foreach(var number in numbers)
    {
        foreach(var board in boards)
        {
            board.Play(number);
        }

        var winningBoards = boards.AsParallel().Where(x => x.IsWinner()).ToArray();
        if (winningBoards.Any())
        {
            return (winningBoards, number);
        }
    }
    return (Enumerable.Empty<BingoBoard>(), 0);
}

(IEnumerable<BingoBoard> winningBoards, int lastNumberCalled) FindLastWinningBoards(IEnumerable<int> numbers, IEnumerable<BingoBoard> boards)
{
    IEnumerable<BingoBoard> lastWinners = Enumerable.Empty<BingoBoard>();
    var lastWinningNumber = 0;

    var winnableBoards = boards.ToList();

    foreach (var number in numbers)
    {
        if (winnableBoards.Count == 0)
        {
            break;
        }

        foreach (var b in winnableBoards)
        {
            b.Play(number);
        }

        var winners = winnableBoards
            .Where(x => x.IsWinner()).ToArray();

        if (winners.Any())
        {
            lastWinners = winners;
            lastWinningNumber = number;

            foreach(var winner in winners)
            {
                winnableBoards.Remove(winner);
            }
        }
    }
    return (lastWinners, lastWinningNumber);
}

class BingoBoard
{
    public int[][]? Numbers { get; set; }
    public bool[][] Marks { get; set; }

    public static BingoBoard Parse(ReadOnlySpan<string> lines)
    {
        if (lines.Length != 5)
        {
            throw new ArgumentException(message: $"The span passed to {nameof(BingoBoard)}.{nameof(Parse)} must have length {Scale}.", paramName: nameof(lines));
        }
        var i = 0;
        return new BingoBoard
        {
            Numbers = new int[Scale][] {
                ParseLine(lines[i++]),
                ParseLine(lines[i++]),
                ParseLine(lines[i++]),
                ParseLine(lines[i++]),
                ParseLine(lines[i++]),
            }
        };

        static int[] ParseLine(string s) =>
            s.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x))
                .ToArray();
    }

    public BingoBoard()
    {
        Marks = new bool[][] {
            new bool[] { false, false, false, false, false },
            new bool[] { false, false, false, false, false },
            new bool[] { false, false, false, false, false },
            new bool[] { false, false, false, false, false },
            new bool[] { false, false, false, false, false },
        };
    }

    public void Play(int number)
    {
        if (Numbers is null)
        {
            throw new NullReferenceException($"{nameof(Numbers)} is null.");
        }
        for (var rowIndex = 0; rowIndex < Numbers.Length; rowIndex++)
        {
            for (var colIndex = 0; colIndex < Numbers[rowIndex].Length; colIndex++)
            {
                if (Numbers[rowIndex][colIndex] == number)
                {
                    Marks[rowIndex][colIndex] = true;
                    return;
                }
            }
        }
    }

    public bool IsWinner()
    {
        for (var i = 0; i < Marks.Length; i++)
        {
            if (RowIsWinner(i) || ColIsWinner(i))
            {
                return true;
            }
        }
        return false;

        bool ColIsWinner(int col) =>
            Marks[0][col] &&
            Marks[1][col] &&
            Marks[2][col] &&
            Marks[3][col] &&
            Marks[4][col];

        bool RowIsWinner(int row) =>
            Marks[row][0] &&
            Marks[row][1] &&
            Marks[row][2] &&
            Marks[row][3] &&
            Marks[row][4];
    }

    public int SumOfUnmarkedNumbers()
    {
        if (Numbers is null)
        {
            throw new NullReferenceException($"{nameof(Numbers)} is null.");
        }
        var result = 0;
        for (var rowIndex = 0; rowIndex < Marks.Length; rowIndex++)
        {
            for (var colIndex = 0; colIndex < Marks[rowIndex].Length; colIndex++)
            {
                if (!Marks[rowIndex][colIndex]) {
                    result += Numbers[rowIndex][colIndex];
                }
            }
        }
        return result;
    }

    public const int Scale = 5;
}