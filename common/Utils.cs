namespace common
{
    public static class Utils
    {
        public static ParallelQuery<int> GetInputAsIntegers(string fileName) => GetLines(fileName)
            .AsParallel()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .AsOrdered() // PLINQ is not guaranteed to be ordered unlike linq to objects
            .Select(x => int.Parse(x));

        /// <summary>
        /// Gets the lines from the specified file relative to the project folder, removing empty lines
        /// </summary>
        public static string[] GetLines(string fileName) =>
            File.ReadAllText(Path.Combine(ProjectFolder(), fileName))
            .Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        public static string ProjectFolder() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        public static short[] ParseCharsToShorts(this string str)
        {
            var ss = str.AsSpan();
            var result = new short[str.Length];
            for (var i = 0; i < ss.Length; i++)
            {
                result[i] = short.Parse(ss.Slice(i, 1));
            }
            return result;
        }

        /// <summary>
        /// Will parse a hexidecimal char (0-9, a-f, A-F) to a nibble value with the least significant
        /// bits first.
        /// </summary>
        public static byte CharToNibbleLE(char c)
        {
            switch (c)
            {
                case '0':
                    return 0b0000;
                case '1':
                    return 0b0001;
                case '2':
                    return 0b0010;
                case '3':
                    return 0b0011;
                case '4':
                    return 0b0100;
                case '5':
                    return 0b0101;
                case '6':
                    return 0b0110;
                case '7':
                    return 0b0111;
                case '8':
                    return 0b1000;
                case '9':
                    return 0b1001;
                case 'A':
                case 'a':
                    return 0b1010;
                case 'B':
                case 'b':
                    return 0b1011;
                case 'C':
                case 'c':
                    return 0b1100;
                case 'D':
                case 'd':
                    return 0b1101;
                case 'E':
                case 'e':
                    return 0b1110;
                case 'F':
                case 'f':
                    return 0b1111;
                default:
                    throw new NotSupportedException($"Expected A-F, a-f, or 0-9.  Got '${c}'.");
            }
        }

        /// <summary>
        /// Will parse a hexidecimal char (0-9, a-f, A-F) to a nibble value with the most significant
        /// bits first.
        /// </summary>
        public static byte CharToNibbleBE(char c)
        {
            switch (c)
            {
                case '0':
                    return 0b0000;
                case '1':
                    return 0b1000;
                case '2':
                    return 0b0100;
                case '3':
                    return 0b1100;
                case '4':
                    return 0b0010;
                case '5':
                    return 0b1010;
                case '6':
                    return 0b0110;
                case '7':
                    return 0b1110;
                case '8':
                    return 0b0001;
                case '9':
                    return 0b1001;
                case 'A':
                case 'a':
                    return 0b0101;
                case 'B':
                case 'b':
                    return 0b1101;
                case 'C':
                case 'c':
                    return 0b0011;
                case 'D':
                case 'd':
                    return 0b1011;
                case 'E':
                case 'e':
                    return 0b0111;
                case 'F':
                case 'f':
                    return 0b1111;
                default:
                    throw new NotSupportedException($"Expected A-F, a-f, or 0-9.  Got '${c}'.");
            }
        }

    }
}