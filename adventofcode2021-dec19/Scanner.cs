using System.Text.RegularExpressions;

namespace adventofcode2021_dec19
{
    public class Scanner
    {
        public int Id { get; init; }
        public List<Point3d> Points { get; private set; } = new();

        private static readonly Regex ScannerRegex = new("--- scanner (?<ScannerId>-?\\d*) ---");

        public static List<Scanner> ParseInput(string[] input)
        {
            var result = new List<Scanner>();
            Scanner current = null;
            foreach (var line in input)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var match = ScannerRegex.Match(line);
                if (match.Success)
                {
                    current = new Scanner { Id = int.Parse(match.Groups["ScannerId"].Value) };
                    result.Add(current);
                }
                else
                {
                    current!.Points.Add(Point3d.Parse(line));
                }
            }
            return result;
        }
    }
}
