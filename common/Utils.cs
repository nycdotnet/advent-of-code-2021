namespace common
{
    public static class Utils
    {
        public static int[] GetInputAsIntegerArray(string fileName) => GetLines(fileName)
            .AsParallel()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => int.Parse(x))
            .ToArray();

        public static string[] GetLines(string fileName) =>
            File.ReadAllText(Path.Combine(ProjectFolder(), fileName))
            .Split("\r\n");

        public static string ProjectFolder() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
    }
}