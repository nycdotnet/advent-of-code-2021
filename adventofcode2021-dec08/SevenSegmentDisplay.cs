using System.Collections.Immutable;
using System.Numerics;

namespace adventofcode2021_dec08
{
    public class SevenSegmentDisplay
    {
        public SevenSegmentDisplay()
        {
            state = NumberToState[0];
        }
        public SevenSegmentDisplay(int number)
        {
             Number = number;
        }
        public SevenSegmentDisplay(IEnumerable<char> segments)
        {
            foreach (var segment in segments)
            {
                switch (segment)
                {
                    case 'a':
                        SegmentA = true;
                        break;
                    case 'b':
                        SegmentB = true;
                        break;
                    case 'c':
                        SegmentC = true;
                        break;
                    case 'd':
                        SegmentD = true;
                        break;
                    case 'e':
                        SegmentE = true;
                        break;
                    case 'f':
                        SegmentF = true;
                        break;
                    case 'g':
                        SegmentG = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private byte state;
        public bool SegmentA
        {
            get => (state & SegmentAMask) > 0;
            set => state |= SegmentAMask;
        }
        public bool SegmentB
        {
            get => (state & SegmentBMask) > 0;
            set => state |= SegmentBMask;
        }
        public bool SegmentC
        {
            get => (state & SegmentCMask) > 0;
            set => state |= SegmentCMask;
        }
        public bool SegmentD
        {
            get => (state & SegmentDMask) > 0;
            set => state |= SegmentDMask;
        }
        public bool SegmentE
        {
            get => (state & SegmentEMask) > 0;
            set => state |= SegmentEMask;
        }
        public bool SegmentF
        {
            get => (state & SegmentFMask) > 0;
            set => state |= SegmentFMask;
        }
        public bool SegmentG
        {
            get => (state & SegmentGMask) > 0;
            set => state |= SegmentGMask;
        }

        public int Number
        {
            get => StateToNumber[state];
            set {
                if (value < 0 || value > 9)
                {
                    throw new ArgumentOutOfRangeException(paramName: nameof(Number), message: $"Can't set a seven segment display to value {value}");
                }
                state = NumberToState[value];
            }
        }

        public void ConsoleRender()
        {
            var originalBackground = Console.BackgroundColor;
            var originalForeground = Console.ForegroundColor;

            Render1();
            Render2();
            Render2();
            Render3();
            Render4();
            Render4();
            Render5();
            
            void Render1() => ConditionalConsoleWrite(SegmentA, " aaaa \n", " .... \n");
            void Render2()
            {
                ConditionalConsoleWrite(SegmentB, "b");
                Console.Write("    ");
                ConditionalConsoleWrite(SegmentC, "c\n", ".\n");
            }
            void Render3() => ConditionalConsoleWrite(SegmentD, " dddd \n", " .... \n");
            void Render4()
            {
                ConditionalConsoleWrite(SegmentE, "e");
                Console.Write("    ");
                ConditionalConsoleWrite(SegmentF, "f\n", ".\n");
            }
            void Render5() => ConditionalConsoleWrite(SegmentG, " gggg \n", " .... \n");

            void ConditionalConsoleWrite(bool condition, string ifOn, string ifOff = ".")
            {
                if (condition)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(ifOn);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(ifOff);
                }
            }

            Console.BackgroundColor = originalBackground;
            Console.ForegroundColor = originalForeground;
        }

        public byte State => state;

        private static readonly Dictionary<int, byte> NumberToState = new() {
            { 0, 119 },
            { 1, 36 },
            { 2, 93 },
            { 3, 109 },
            { 4, 46 },
            { 5, 107 },
            { 6, 123 },
            { 7, 37 },
            { 8, 127 },
            { 9, 111 }
        };

        private static readonly Dictionary<byte, int> StateToNumber = NumberToState
            .ToDictionary(x => x.Value, x => x.Key);

        public int Cardinality()
        {
            return BitOperations.PopCount(State);
        }

        /// <summary>
        /// If a seven segment display shows these number of segments on, then the value must
        /// be one of the returned sets of integers.
        /// </summary>
        public static readonly ImmutableDictionary<int, int[]> CardinalityToPossibleValues =
            new Dictionary<int, int[]> {
                { 2, new int[] { 1 } },
                { 3, new int[] { 7 } },
                { 4, new int[] { 4 } },
                { 5, new int[] { 2, 3, 5 } },
                { 6, new int[] { 0, 6, 9 } },
                { 7, new int[] { 8 } },
            }
            .ToImmutableDictionary();

        // inspiration from https://stackoverflow.com/questions/4854207/get-a-specific-bit-from-byte
        const byte SegmentAMask = 1 << 0;
        const byte SegmentBMask = 1 << 1;
        const byte SegmentCMask = 1 << 2;
        const byte SegmentDMask = 1 << 3;
        const byte SegmentEMask = 1 << 4;
        const byte SegmentFMask = 1 << 5;
        const byte SegmentGMask = 1 << 6;
    }
}
