using System.Diagnostics;
using static common.Utils;

Console.WriteLine("Day 10: Syntax Scoring");

//var inputTextFileName = "example-input1.txt";
var inputTextFileName = "myPuzzleInput.txt";


var answer1 = GetErrorScore(inputTextFileName);
Debug.Assert(answer1 == 323613);

var answer2 = GetAutocompleteScore(inputTextFileName);
Debug.Assert(answer2 == 3103006161L);

int GetErrorScore(string fileName)
{
    var errorScore = 0;
    var lines = GetLines(fileName).ToArray();
    for (var i = 0; i < lines.Length; i++)
    {
        var result = Chunk.Parse(lines[i]);
        if (result.result == Chunk.ChunkParseResult.Corrupted)
        {
            errorScore += result.found switch
            {
                ')' => 3,
                ']' => 57,
                '}' => 1197,
                '>' => 25137,
                _ => throw new NotSupportedException()
            };
        }
    }

    Console.WriteLine($"Total syntax error score for part 1 is {errorScore}.");

    return errorScore;
}

long GetAutocompleteScore(string fileName)
{
    var scores = new List<long>();
    var lines = GetLines(fileName).ToArray();
    for (var i = 0; i < lines.Length; i++)
    {
        var result = Chunk.Parse(lines[i]);
        if (result.result == Chunk.ChunkParseResult.Incomplete)
        {
            var autoCompleteScore = 0L;
            while (result.stack.TryPop(out var token))
            {
                var add = token.ExpectedClosingChar switch
                {
                    ')' => 1,
                    ']' => 2,
                    '}' => 3,
                    '>' => 4,
                    _ => throw new NotSupportedException()
                };
                autoCompleteScore = (autoCompleteScore * 5) + add;
            }
            scores.Add(autoCompleteScore);
        }
    }

    var skipCount = scores.Count / 2;
    var middleScore = scores.OrderBy(s => s).Skip(skipCount).Take(1).Single();

    Console.WriteLine($"Middle autocomplete score for part 2 is {middleScore}.");

    return middleScore;
}





public record Chunk
{
    public char OpeningChar { get; init; }
    public char ExpectedClosingChar => OpeningChar switch
    {
        '(' => ')',
        '[' => ']',
        '{' => '}',
        '<' => '>',
        _ => throw new NotImplementedException()
    };

    public Chunk(char opening)
    {
        OpeningChar = opening;
    }

    public static (ChunkParseResult result, Stack<Chunk> stack, char found) Parse(ReadOnlySpan<char> input)
    {
        var chunks = new Stack<Chunk>();
        for (var i = 0; i < input.Length; i++)
        {
            var token = input[i];
            if (IsOpeningChar(token))
            {
                chunks.Push(new Chunk(token));
            }
            else
            {
                if (token == chunks.Peek().ExpectedClosingChar)
                {
                    chunks.Pop();
                }
                else
                {
                    return (ChunkParseResult.Corrupted,
                        stack: chunks,
                        found: token);
                }
            }
        }

        if (chunks.Count == 0)
        {
            return (ChunkParseResult.Valid,
                stack: chunks,
                found: '\0');
        }
        return (ChunkParseResult.Incomplete,
            stack: chunks,
            found: '\0');
    }



    public static bool IsOpeningChar(char c) => c == '(' || c == '[' || c == '{' || c == '<';

    public enum ChunkParseResult
    {
        Undefined,
        Valid,
        Incomplete,
        Corrupted
    }
}