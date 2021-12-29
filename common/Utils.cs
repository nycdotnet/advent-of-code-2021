namespace common
{
    public static class Utils
    {
        public static ParallelQuery<int> GetInputAsIntegers(string fileName) => GetLines(fileName)
            .AsParallel()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .AsOrdered() // PLINQ is not guaranteed to be ordered unlike linq to objects
            .Select(x => int.Parse(x));

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
    }
}