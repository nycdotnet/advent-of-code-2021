using adventofcode2021_dec08;
using System.Diagnostics;
using static common.Utils;


Console.WriteLine("Day 8: Seven Segment Search");

//var inputTextFileName = "example-input1.txt";
//var inputTextFileName = "example-input2.txt";
var inputTextFileName = "myPuzzleInput.txt";


var answer1 = SevenSegmentSearchPart1(inputTextFileName);
Debug.Assert(answer1 == 245);

var answer2 = SevenSegmentSearchPart2(inputTextFileName);
Debug.Assert(answer2 == 983026);


int SevenSegmentSearchPart1(string file)
{
    var displays = GetLines(file)
        .Select(line => SevenSegmentObservation.Parse(line))
        .ToArray();

    var countOf1_4_7_8 = 0;
    foreach (var display in displays)
    {
        countOf1_4_7_8 += display
            .CurrentReading!
            .Count(r =>
                SevenSegmentDisplay.CardinalityToPossibleValues[r.Length].Length == 1);
    }

    return countOf1_4_7_8;
}

int SevenSegmentSearchPart2(string file)
{
    var displays = GetLines(file)
        .Select(line => SevenSegmentObservation.Parse(line))
        .ToArray();

    var sum = 0;
    foreach (var display in displays)
    {
        var map = display.Decipher();
        var decipheredDisplays = display.ApplyMap(map);
        var number = decipheredDisplays[0].Number * 1000 +
            decipheredDisplays[1].Number * 100 +
            decipheredDisplays[2].Number * 10 +
            decipheredDisplays[3].Number;
        sum += number;
    }

    return sum;
}

record EasySevenSegmentDecodes
{
    public string One { get; set; }
    public string Four { get; set; }
    public string Seven { get; set; }
    public string Eight { get; set; }

    public static EasySevenSegmentDecodes FromObservedDigits(string[] digits)
    {
        var result = new EasySevenSegmentDecodes();
        foreach (var digit in digits)
        {
            switch (digit.Length)
            {
                case 2:
                    result.One = digit;
                    break;
                case 3:
                    result.Seven = digit;
                    break;
                case 4:
                    result.Four = digit;
                    break;
                case 7:
                    result.Eight = digit;
                    break;
                default:
                    break;
            }
        }
        return result;
    }
}


record SevenSegmentObservation()
{
    public string[]? ObservedDigits { get; set; }
    public string[]? CurrentReading { get; set; }
    public static SevenSegmentObservation Parse(string input)
    {
        var x = input.Split(' ');
        var result = new SevenSegmentObservation {
            ObservedDigits = x.Take(10).ToArray(),
            CurrentReading = x.Skip(11).ToArray()
        };
        return result;
    }

    public SevenSegmentDisplay[] ApplyMap(Dictionary<char, char> decipher)
    {
        if (CurrentReading == null)
        {
            throw new ArgumentNullException(paramName: nameof(CurrentReading), message: $"The property {nameof(CurrentReading)} must not be null.");
        }
        return CurrentReading.Select(r => new SevenSegmentDisplay(r.Select(s => decipher[s]))).ToArray();
    }

    /// <summary>
    /// Returns a dictionary of what each observed stroke should map to
    /// </summary>
    public Dictionary<char, char> Decipher()
    {
        if (ObservedDigits == null)
        {
            throw new ArgumentNullException(paramName: nameof(ObservedDigits), message: $"The property {nameof(ObservedDigits)} must not be null.");
        }
        var easy = EasySevenSegmentDecodes.FromObservedDigits(ObservedDigits);

        // We can find 'a' by seeing the unique stroke from the two and three stroke numbers (which are 1 and 7).
        var a = easy.Seven.Where(c => !easy.One.Contains(c)).Single();

        // since we now know 'a', we can narrow down 'd' and 'g' to two possibilities using the 5 stroke numbers
        // which each have 'a',  'd', and 'g' by excluding the item we've decided is 'a'.

        var fiveStrokeNumbers = ObservedDigits.Where(d => d.Length == 5).ToArray();
        var strokesInAllFiveStrokeNumbersExcludingA = fiveStrokeNumbers[0]
            .Intersect(fiveStrokeNumbers[1])
            .Intersect(fiveStrokeNumbers[2])
            .Where(x => x != a).ToArray();

        // All of the six stroke numbers have 'g', but only two have 'd'.  We can use this to find 'g' and 'd'.
        var sixStrokeNumbers = ObservedDigits!.Where(d => d.Length == 6).ToArray();

        var g = strokesInAllFiveStrokeNumbersExcludingA
            .Where(x => sixStrokeNumbers.All(n6 => n6.Contains(x)))
            .Single();

        var d = strokesInAllFiveStrokeNumbersExcludingA.Where(x => x != g).Single();

        // we now know 'a', 'd', 'g'.  We can get 'b' by looking at Four and excluding One and 'd'.
        var b = easy.Four.Except(easy.One).Where(x => x != d).Single();

        // we now know 'a', 'b', 'd', 'g'.

        // of the six stroke numbers, eliminate Zero which lacks 'd'.  'c' and 'f' can be determined with the remaining two
        // six stroke numbers. Since the mappings for 'c' and 'f' are in One, and we just don't know which is which,
        // we can use Six and Nine to help - Six will lack one of them (which will be 'c') and Nine will have both.  The
        // one that is not 'c' in One is 'f'.

        var six = sixStrokeNumbers
            .Where(n6 => n6.Contains(d))
            .Where(n6 => n6.Intersect(easy.One).Count() == 1)
            .Single();

        var c = easy.One.Where(x => !six.Contains(x)).Single();

        var f = easy.One.Where(x => x != c).Single();

        // we now know 'a', 'b', 'c', 'd', 'f', and 'g'; 'e' will be the remaining value.
        var e = easy.Eight.Where(x => x != a && x != b && x != c && x != d && x != f && x != g).Single();

        return new Dictionary<char, char> {
            { a, 'a' },
            { b, 'b' },
            { c, 'c' },
            { d, 'd' },
            { e, 'e' },
            { f, 'f' },
            { g, 'g' },
        };
    }
}




//var display = new SevenSegmentDisplay();

//for (var i = 0; i < 10; i++)
//{
//    display.SetNumber(i);
//    display.ConsoleRender();
//    Console.WriteLine($"Cardinality for {i}: {display.Cardinality()}");
//    Console.WriteLine();
//}
