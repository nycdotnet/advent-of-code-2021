namespace common
{
    public static class Utils
    {
        public static ParallelQuery<int> GetInputAsIntegers(string fileName) => GetLines(fileName)
            .AsParallel()
            .AsOrdered() // PLINQ is not guaranteed to be ordered unlike linq to objects
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => int.Parse(x));

        public static string[] GetLines(string fileName) =>
            File.ReadAllText(Path.Combine(ProjectFolder(), fileName))
            .Split("\r\n");

        public static string ProjectFolder() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
    }
}