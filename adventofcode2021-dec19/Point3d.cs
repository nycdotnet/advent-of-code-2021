namespace adventofcode2021_dec19
{
    public struct Point3d
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Z { get; init; }
        public static Point3d Parse(string input)
        {
            var nums = input.Split(',');
            return new Point3d
            {
                X = int.Parse(nums[0]),
                Y = int.Parse(nums[1]),
                Z = nums.Length > 2 ? int.Parse(nums[2]) : 0
            };
        }
    }
}
