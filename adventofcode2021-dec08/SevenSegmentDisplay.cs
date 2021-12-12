namespace adventofcode2021_dec08
{
    public class SevenSegmentDisplay
    {
        public SevenSegmentDisplay()
        {
            state = NumberStates[0];
        }
        public SevenSegmentDisplay(int number)
        {
            SetNumber(number);
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
        public void SetNumber(int number)
        {
            if (number < 0 || number > 9)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(number), message: $"Can't set a seven segment display to value {number}");
            }
            state = NumberStates[number];
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

        private static readonly Dictionary<int, byte> NumberStates = new() {
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
