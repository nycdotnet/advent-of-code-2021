using System.Runtime.InteropServices;
using System.Text;
using static adventofcode2021_dec18.SnailfishNumber;

namespace adventofcode2021_dec18
{
    public class SnailfishNumber
    {
        public Pair OuterPair { get; private set; }
        private List<Pair> _Pairs { get; set; }
        public IEnumerable<Pair> GetPairs() => _Pairs;
        public void AddPair(Pair pair) => _Pairs.Add(pair);

        public enum Side
        {
            Unspecified = 0,
            Left = 1,
            Right = 2
        }

        public static SnailfishNumber Parse(string s)
        {
            var result = new SnailfishNumber {
                _Pairs = new()
            };
            result.OuterPair = Pair.Parse(s, result, out int _, 0, null);
            
            return result;
        }

        public void Reduce(int maxIterations = -1)
        {
            int iterations = 0;
            while(TryReduce())
            {
                iterations += 1;
                Reindex();
                if (iterations == maxIterations)
                {
                    return;
                }
            }
        }

        private void Reindex()
        {
            _Pairs.Clear();
            OuterPair.Index();
        }

        private bool TryReduce()
        {
            var explodeResult = OuterPair.TryExplode();
            if (explodeResult)
            {
                return true;
            }
            var splitResult = OuterPair.TrySplit();
            if (splitResult)
            {
                var newNumber = Parse(ToString());
                OuterPair = newNumber.OuterPair;
                OuterPair.Rehome(this);
                return true;
            }
            return false;
        }

        public static SnailfishNumber AddWithoutReduce(SnailfishNumber a, SnailfishNumber b)
        {
            var resultString = $"[{a},{b}]";
            return Parse(resultString);
        }

        public override string ToString() => OuterPair.ToString();

        public class Pair
        {
            /// <summary>
            /// Reference to the <see cref="Pair"/> that contains this <see cref="Pair"/>.  Null if this is the
            /// root <see cref="Pair"/>.
            /// </summary>
            public Pair? Parent { get; init; }
            /// <summary>
            /// The side of the parent <see cref="Pair"/> of this <see cref="Pair"/>.  Null if this is the
            /// root <see cref="Pair"/>.
            /// </summary>
            public Side? Side { get; private set; } = SnailfishNumber.Side.Unspecified;

            /// <summary>
            /// Reference to the outer <see cref="SnailfishNumber"/>.
            /// </summary>
            public SnailfishNumber Number { get; internal set; }
            /// <summary>
            /// The zero-based index of the [ character of this pair.
            /// </summary>
            public int StartIndex { get; private set; }
            /// <summary>
            /// The zero-based index of the ] character of this pair.
            /// </summary>
            public int EndIndex { get; private set; }

            public object Left { get; internal set; }
            public object Right { get; internal set; }

            /// <summary>
            /// Enumerates the parents of this <see cref="Pair"/> in order starting from the
            /// closest ancestor (the direct parent)
            /// </summary>
            public IEnumerable<Pair> Parents()
            {
                var next = Parent;
                while (next != null)
                {
                    yield return next;
                    next = next.Parent;
                }
            }

            public IEnumerable<(int index, Side side)> NumberIndexes()
            {
                if (Left is int)
                {
                    yield return (StartIndex + 1, SnailfishNumber.Side.Left);
                }
                if (Right is int)
                {
                    yield return (EndIndex - 1, SnailfishNumber.Side.Right);
                }
            }

            public bool IsNestedInFourOrMorePairs() => Parent?.Parent?.Parent?.Parent != null;

            public bool TryGetRightAsPair(out Pair? right)
            {
                if (Right is Pair r)
                {
                    right = r;
                    return true;
                }
                right = null;
                return false;
            }

            public bool TryGetLeftAsPair(out Pair? left)
            {
                if (Left is Pair l)
                {
                    left = l;
                    return true;
                }
                left = null;
                return false;
            }

            public Pair? LeftAsPair
            {
                get => Left is Pair left ? left : null;
                set => Left = value;
            }
            public Pair? RightAsPair
            {
                get => Right is Pair right ? right : null;
                set => Right = value;
            }

            public int? LeftAsInt
            {
                get => Left is int left ? left : null;
                set => Left = value;
            }
            public int? RightAsInt
            {
                get => Right is int right ? right : null;
                set => Right = value;
            }

            public override string ToString()
            {
                var result = new StringBuilder(5);
                result.Append('[');
                if (Left is Pair x)
                {
                    result.Append(x.ToString());
                }
                else if (Left is int x2)
                {
                    result.Append(x2.ToString());
                }
                else
                {
                    throw new NotSupportedException("X was neither int nor Pair.");
                }
                result.Append(',');
                if (Right is Pair y)
                {
                    result.Append(y.ToString());
                }
                else if (Right is int y2)
                {
                    result.Append(y2.ToString());
                }
                else
                {
                    throw new NotSupportedException("Y was neither int nor Pair.");
                }
                result.Append(']');
                return result.ToString();
            }

            public static Pair Parse(string s, SnailfishNumber n, out int charactersProcessed, int startIndex = 0, Pair? parent = null)
            {
                if (s[startIndex] != '[')
                {
                    throw new InvalidDataException($"Snailfish pairs must start with [.  Problem at position {startIndex} found {s[startIndex]}.");
                }
                charactersProcessed = 1; // for the outer '['
                var isX = true;
                var result = new Pair
                {
                    Parent = parent,
                    Number = n
                };

                var chars = new List<char>();
                for (var i = startIndex + 1; i < s.Length; i++)
                {
                    var c = s[i];
                    charactersProcessed += 1;
                    switch (c)
                    {
                        case '[':
                            // start of a new snailfish pair
                            var parsed = Parse(s, n, out var parsedCount, i, result);
                            i += parsedCount - 1; // minus 1 because we don't want to double-count this [.
                            charactersProcessed += parsedCount - 1;

                            if (isX)
                            {
                                parsed.Side = SnailfishNumber.Side.Left;
                                result.Left = parsed;
                            }
                            else
                            {
                                parsed.Side = SnailfishNumber.Side.Right;
                                result.Right = parsed;
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
                            if (result.Left == null) // this will be null if X wasn't already assigned as a pair.
                            {
                                result.Left = int.Parse(CollectionsMarshal.AsSpan(chars));
                                result.StartIndex = i - chars.Count - 1;
                            }
                            chars.Clear();
                            break;
                        case ']':
                            if (isX)
                            {
                                throw new InvalidDataException($"Did not expect ] at position {startIndex + 1}.");
                            }
                            if (result.Right == null)
                            {
                                result.Right = int.Parse(CollectionsMarshal.AsSpan(chars));
                            }
                            result.EndIndex = i;
                            result.Number.AddPair(result);
                            return result;
                        default:
                            throw new NotSupportedException($"{nameof(Pair)} does not support parsing the {c} character.");
                    }
                }
                throw new NotSupportedException("Unexpected end of content.");
            }

            internal bool TryExplode()
            {
                if (IsNestedInFourOrMorePairs())
                {
                    // explode this pair!
                    //To explode a pair, the pair's left value is added to the first regular number
                    //to the left of the exploding pair (if any), and the pair's right value is
                    //added to the first regular number to the right of the exploding pair(if any).
                    //Exploding pairs will always consist of two regular numbers. Then, the entire
                    //exploding pair is replaced with the regular number 0.

                    var allNumbers = Number.GetPairs()
                        .Where(p => p != this)
                        .SelectMany(p => p.NumberIndexes().Select(ni => (pair: p, info: ni))).ToArray();

                    var leftNumber = allNumbers
                        .Where(n => n.info.index < StartIndex)
                        .OrderByDescending(n => n.info.index).Take(1).SingleOrDefault();

                    if (leftNumber.info.side == SnailfishNumber.Side.Right)
                    {
                        leftNumber.pair.Right = (int)leftNumber.pair.Right + LeftAsInt!;
                    }
                    else if (leftNumber.info.side == SnailfishNumber.Side.Left)
                    {
                        leftNumber.pair.Left = (int)leftNumber.pair.Left + LeftAsInt!;
                    }

                    var rightNumber = allNumbers
                        .Where(n => n.info.index > EndIndex)
                        .OrderBy(n => n.info.index).Take(1).SingleOrDefault();

                    if (rightNumber.info.side == SnailfishNumber.Side.Left)
                    {
                        rightNumber.pair.Left = (int)rightNumber.pair.Left + RightAsInt!;
                    }
                    else if (rightNumber.info.side == SnailfishNumber.Side.Right)
                    {
                        rightNumber.pair.Right = (int)rightNumber.pair.Right + RightAsInt!;
                    }


                    ReplaceThisInParentWithZero();
                    return true;
                }

                if (Left is Pair left)
                {
                    var reduced = left.TryExplode();
                    if (reduced)
                    {
                        return true;
                    }
                }

                if (Right is Pair right)
                {
                    var reduced = right.TryExplode();
                    if (reduced)
                    {
                        return true;
                    }
                }

                return false;

                void ReplaceThisInParentWithZero()
                {
                    if (Side == SnailfishNumber.Side.Left)
                    {
                        Parent.Left = 0;
                    }
                    else if (Side == SnailfishNumber.Side.Right)
                    {
                        Parent.Right = 0;
                    }
                    else
                    {
                        throw new NotImplementedException("Side must be left or right.");
                    }
                }
            }

            internal bool TrySplit()
            {
                if (Left is int leftInt && leftInt >= 10)
                {
                    Left = new Pair
                    {
                        Left = (int)Math.Floor(leftInt / 2.0),
                        Right = (int)Math.Ceiling(leftInt / 2.0),
                        Number = Number,
                        Parent = this,
                        Side = SnailfishNumber.Side.Left,
                    };
                    return true;
                }

                if (Right is int rightInt && rightInt >= 10)
                {
                    Right = new Pair
                    {
                        Left = (int)Math.Floor(rightInt / 2.0),
                        Right = (int)Math.Ceiling(rightInt / 2.0),
                        Number = Number,
                        Parent = this,
                        Side = SnailfishNumber.Side.Right
                    };
                    return true;
                }

                if (Left is Pair leftPair)
                {
                    var split = leftPair.TrySplit();
                    if (split)
                    {
                        return true;
                    }
                }

                if (Right is Pair rightPair)
                {
                    var split = rightPair.TrySplit();
                    if (split)
                    {
                        return true;
                    }
                }

                return false;
            }

            internal void Index()
            {
                Number.AddPair(this);
                if (Left is Pair left)
                {
                    left.Index();
                }
                if (Right is Pair right)
                {
                    right.Index();
                }
            }

            internal void Rehome(SnailfishNumber snailfishNumber)
            {
                Number = snailfishNumber;
                if (Left is Pair left)
                {
                    left.Rehome(snailfishNumber);
                }
                if (Right is Pair right)
                {
                    right.Rehome(snailfishNumber);
                }
            }
        }
    }
}
