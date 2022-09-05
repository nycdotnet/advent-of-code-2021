using FluentAssertions;
using Xunit;

namespace tests
{
    public class Day18Tests
    {
        [Fact]
        public void ParsingSimpleWorks()
        {
            SnailfishPair.TryParse("[1,2]", out var n, out var l).Should().Be(true);
            l.Should().Be(5);
            n.ToString().Should().Be("[1,2]");
        }

        [Fact]
        public void ParsingComplexWorks()
        {
            SnailfishPair.TryParse("[[[[4,3],4],4],[7,[[8,4],9]]]", out var n, out var l).Should().Be(true);
            l.Should().Be(29);
            n.ToString().Should().Be("[[[[4,3],4],4],[7,[[8,4],9]]]");
        }
    }
}
