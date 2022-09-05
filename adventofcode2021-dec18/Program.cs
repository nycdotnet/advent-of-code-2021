using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;


int charsRead;
int xi;
int yi;
SnailfishPair pair;
SnailfishPair pair2;
bool parseResult;
string content;

//content = "[1,2]";
//parseResult = SnailfishPair.TryParse(content, out pair, out charsRead);
//Debug.Assert(parseResult);
//Debug.Assert(pair.TryGetIntX(out xi));
//Debug.Assert(xi == 1);
//Debug.Assert(pair.TryGetIntY(out yi));
//Debug.Assert(yi == 2);
//Debug.Assert(charsRead == content.Length);

//content = "[[1,2],3]";
//parseResult = SnailfishPair.TryParse(content, out pair, out charsRead);
//Debug.Assert(parseResult);
//Debug.Assert(pair.TryGetPairX(out pair2));
//Debug.Assert(pair2.TryGetIntX(out xi));
//Debug.Assert(xi == 1);
//Debug.Assert(pair2.TryGetIntY(out yi));
//Debug.Assert(yi == 2);
//Debug.Assert(charsRead == content.Length);

//content = "[9,[8,7]]";
//parseResult = SnailfishPair.TryParse(content, out pair, out charsRead);
//Debug.Assert(parseResult);
//Debug.Assert(pair.TryGetIntX(out xi));
//Debug.Assert(xi == 9);
//Debug.Assert(pair.TryGetPairY(out pair2));
//Debug.Assert(pair2.TryGetIntX(out xi));
//Debug.Assert(pair2.TryGetIntY(out yi));
//Debug.Assert(xi == 8);
//Debug.Assert(yi == 7);
//Debug.Assert(charsRead == content.Length);

//content = "[[[[1,3],[5,3]],[[1,3],[8,7]]],[[[4,9],[6,9]],[[8,2],[7,3]]]]";
//parseResult = SnailfishPair.TryParse(content, out pair, out charsRead);


_ = SnailfishPair.TryParse("[[[[4,3],4],4],[7,[[8,4],9]]]", out var addend1, out _);
_ = SnailfishPair.TryParse("[1,1]", out var addend2, out _);
var sum = SnailfishPair.Add(addend1, addend2);
var sumString = sum.ToString();
Debug.Assert(sumString == "[[[[[4,3],4],4],[7,[[8,4],9]]],[1,1]]");


public record class SnailfishPair
{
    public object X { get; private set; }
    public object Y { get; private set; }
    public SnailfishPair? parent { get; private set; }



    public bool TryGetIntX(out int x)
    {
        if (X is int xvalue)
        {
            x = xvalue;
            return true;
        }
        x = 0;
        return false;
    }

    public bool TryGetPairX(out SnailfishPair? x)
    {
        if (X is SnailfishPair xvalue)
        {
            x = xvalue;
            return true;
        }
        x = null;
        return false;
    }

    public bool TryGetIntY(out int y)
    {
        if (Y is int yvalue)
        {
            y = yvalue;
            return true;
        }
        y = 0;
        return false;
    }

    public bool TryGetPairY(out SnailfishPair? y)
    {
        if (Y is SnailfishPair yvalue)
        {
            y = yvalue;
            return true;
        }
        y = null;
        return false;
    }

    public static SnailfishPair Add(SnailfishPair a, SnailfishPair b)
    {
        var result = new SnailfishPair();
        result.X = a;
        result.Y = b;

        //result.Reduce();

        return result;
    }

    private void Reduce()
    {
        while (true)
        {
            if (!TryReduce(this, 0))
            {
                return;
            }
        }


        bool TryReduce(SnailfishPair pair, int depth)
        {
            if (depth >= 3)
            {
                // destroy pair.


                return true;
            }
            if (pair.TryGetPairX(out var x))
            {
                var result = TryReduce(x, depth + 1);
                if (result)
                {
                    return true;
                }
            }
            if (pair.TryGetPairY(out var y))
            {
                var result = TryReduce(y, depth + 1);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

    }

    public override string ToString()
    {
        var result = "[";

        if (TryGetIntX(out int x))
        {
            result += x.ToString();
        }
        else if (TryGetPairX(out var xPair))
        {
            result += xPair.ToString();
        }
        else
        {
            result += "null";
        }
        result += ",";

        if (TryGetIntY(out int y))
        {
            result += y.ToString();
        }
        else if (TryGetPairY(out var yPair))
        {
            result += yPair.ToString();
        }
        else
        {
            result += "null";
        }
        return result + "]";
    }


    public static bool TryParse(ReadOnlySpan<char> content, out SnailfishPair result, out int charactersProcessed) =>
        TryParse(content, 0, out result, out charactersProcessed, null);

    public static bool TryParse(ReadOnlySpan<char> content, int startIndex, out SnailfishPair result, out int charactersProcessed, SnailfishPair parent)
    {
        if (content[startIndex] != '[')
        {
            throw new InvalidDataException($"Snailfish pairs must start with [.  Problem at position {startIndex + 1} found {content[startIndex]}.");
        }
        var isX = true;
        result = new SnailfishPair();
        result.parent = parent;
        charactersProcessed = 1; // includes opening [
        var chars = new List<char>();
        for (var i = startIndex + 1; i < content.Length; i++)
        {
            var c = content[i];
            charactersProcessed += 1;
            switch (c)
            {
                case '[':
                    // start of a new snailfish pair

                    Debug.Assert(TryParse(content, i, out var parsed, out var parsedCount, result));
                    i += parsedCount - 1; // minus 1 because we don't want to double-count this [.
                    charactersProcessed += parsedCount - 1;

                    if (isX)
                    {
                        result.X = parsed;
                    }
                    else
                    {
                        result.Y = parsed;
                    }
                    break;
                case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                    chars.Add(c);
                    break;
                case ',':
                    if (!isX)
                    {
                        throw new InvalidDataException($"Did not expect , at position {startIndex + 1}.");
                    }
                    isX = false;
                    if (result.X == null) // this will be null if X wasn't already assigned as a pair.
                    {
                        result.X = int.Parse(CollectionsMarshal.AsSpan(chars));
                    }
                    chars.Clear();
                    break;
                case ']':
                    if (isX)
                    {
                        throw new InvalidDataException($"Did not expect ] at position {startIndex + 1}.");
                    }
                    if (result.Y == null)
                    {
                        result.Y = int.Parse(CollectionsMarshal.AsSpan(chars));
                    }
                    return true;
                default:
                    throw new NotSupportedException($"{nameof(SnailfishPair)} does not support parsing the {c} character.");
            }
        }
        throw new NotSupportedException("Unexpected end of content.");
    }
}