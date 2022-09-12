using adventofcode2021_dec18;
using FluentAssertions;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace tests
{
    public class Day18Tests
    {
        [Fact]
        public void ParsingSimpleWorks()
        {
            var n = SnailfishNumber.Parse("[1,2]");
            n.Should().NotBeNull();
            n.ToString().Should().Be("[1,2]");
            n.OuterPair.Parent.Should().BeNull();
            n.OuterPair.StartIndex.Should().Be(0);
            n.OuterPair.EndIndex.Should().Be(4);
            n.OuterPair.IsNestedInFourOrMorePairs().Should().BeFalse();
            n.GetPairs().Count().Should().Be(1);
        }

        [Fact]
        public void StartAndEndPositionWorkAsExpectedForPairOnLeft()
        {
            var n = SnailfishNumber.Parse("[[1,2],3]");
            n.ToString().Should().Be("[[1,2],3]");
            n.OuterPair.LeftAsPair.ToString().Should().Be("[1,2]");
            n.OuterPair.LeftAsPair.StartIndex.Should().Be(1);
            n.OuterPair.LeftAsPair.EndIndex.Should().Be(5);
            n.OuterPair.StartIndex.Should().Be(0);
            n.OuterPair.EndIndex.Should().Be(8);
        }

        [Fact]
        public void StartAndEndPositionWorkAsExpectedForPairOnRight()
        {
            var n = SnailfishNumber.Parse("[1,[2,3]]");
            n.ToString().Should().Be("[1,[2,3]]");
            n.OuterPair.RightAsPair.ToString().Should().Be("[2,3]");
            n.OuterPair.RightAsPair.StartIndex.Should().Be(3);
            n.OuterPair.RightAsPair.EndIndex.Should().Be(7);
            n.OuterPair.StartIndex.Should().Be(0);
            n.OuterPair.EndIndex.Should().Be(8);
        }

        [Fact]
        public void ParsingComplexWorks()
        {
            var n = SnailfishNumber.Parse("[[[[4,3],4],4],[7,[[8,4],9]]]");
            n.Should().NotBeNull();
            n.ToString().Should().Be("[[[[4,3],4],4],[7,[[8,4],9]]]");
            n.GetPairs().Count().Should().Be(7);

            n.OuterPair.TryGetLeftAsPair(out var pairX).Should().BeTrue();
            pairX!.ToString().Should().Be("[[[4,3],4],4]");
            pairX.Parent.Should().Be(n.OuterPair);

            n.OuterPair.TryGetRightAsPair(out var pairY).Should().BeTrue();
            pairY!.ToString().Should().Be("[7,[[8,4],9]]");
            pairY.Parent.Should().Be(n.OuterPair);
        }

        [Fact]
        public void AllPairsHaveReferenceToNumber()
        {
            var n = SnailfishNumber.Parse("[[[[4,3],4],4],[7,[[8,4],9]]]");
            var other = SnailfishNumber.Parse("[1,1]");
            n.Should().NotBeNull();

            n.OuterPair.Number.Should().Be(n);
            n.OuterPair.Number.Should().NotBe(other);
        }

        [Fact]
        public void AddWithoutReduceWorks()
        {
            var a = SnailfishNumber.Parse("[[[[4,3],4],4],[7,[[8,4],9]]]");
            var b = SnailfishNumber.Parse("[1,1]");

            var sum = SnailfishNumber.AddWithoutReduce(a, b);
            sum.ToString().Should().Be("[[[[[4,3],4],4],[7,[[8,4],9]]],[1,1]]");
            sum.GetPairs().Count().Should().Be(9);
        }

        [Fact]
        public void CanFindNestedInFour()
        {
            var a = SnailfishNumber.Parse("[[[[[9,8],1],2],3],4]");

            a.OuterPair.LeftAsPair.LeftAsPair.LeftAsPair.LeftAsPair.LeftAsInt.Should().Be(9);
            a.OuterPair.LeftAsPair.LeftAsPair.LeftAsPair.LeftAsPair.RightAsInt.Should().Be(8);

            a.OuterPair.LeftAsPair.LeftAsPair.LeftAsPair.LeftAsPair.ToString().Should().Be("[9,8]");
            a.OuterPair.LeftAsPair.LeftAsPair.LeftAsPair.LeftAsPair.IsNestedInFourOrMorePairs().Should().BeTrue();
            a.OuterPair.LeftAsPair.LeftAsPair.LeftAsPair.IsNestedInFourOrMorePairs().Should().BeFalse();
            a.OuterPair.LeftAsPair.LeftAsPair.IsNestedInFourOrMorePairs().Should().BeFalse();
            a.OuterPair.LeftAsPair.IsNestedInFourOrMorePairs().Should().BeFalse();
            a.OuterPair.IsNestedInFourOrMorePairs().Should().BeFalse();

        }

        [Fact]
        public void ParentsWorks()
        {
            var a = SnailfishNumber.Parse("[[[[[9,8],1],2],3],4]");

            a.OuterPair.TryGetLeftAsPair(out var a2).Should().BeTrue();
            a2!.TryGetLeftAsPair(out var a3).Should().BeTrue();
            a3!.TryGetLeftAsPair(out var a4).Should().BeTrue();
            a4!.TryGetLeftAsPair(out var a5).Should().BeTrue();

            var p = a5!.Parents().ToArray();
            p.Length.Should().Be(4);
            p[0].ToString().Should().Be("[[9,8],1]");
            p[1].ToString().Should().Be("[[[9,8],1],2]");
            p[2].ToString().Should().Be("[[[[9,8],1],2],3]");
            p[3].ToString().Should().Be("[[[[[9,8],1],2],3],4]");
        }

        [Theory]
        [InlineData("[[[[[9,8],1],2],3],4]", "[[[[0,9],2],3],4]", 4)]
        [InlineData("[7,[6,[5,[4,[3,2]]]]]", "[7,[6,[5,[7,0]]]]", 4)]
        [InlineData("[[6,[5,[4,[3,2]]]],1]", "[[6,[5,[7,0]]],3]", 4)]
        [InlineData("[[3,[2,[1,[7,3]]]],[6,[5,[4,[3,2]]]]]", "[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]", 8)]
        [InlineData("[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]", "[[3,[2,[8,0]]],[9,[5,[7,0]]]]", 7)]
        public void ExplodeExamples(string input, string expectedNumber, int expectedPairCount)
        {
            var a = SnailfishNumber.Parse(input);
            a.Reduce(1);
            a.ToString().Should().Be(expectedNumber);
            a.GetPairs().Count().Should().Be(expectedPairCount);
        }

        [Fact]
        public void SplitOnLeftSideWorks()
        {
            var n = SnailfishNumber.Parse("[11,[1,2]]");
            n.Reduce(1);
            n.ToString().Should().Be("[[5,6],[1,2]]");
        }

        [Fact]
        public void SplitOnRightSideWorks()
        {
            var n = SnailfishNumber.Parse("[[1,2],11]");
            n.Reduce(1);
            n.ToString().Should().Be("[[1,2],[5,6]]");
        }

        [Theory]
        [InlineData("[[[[0,7],4],[15,[0,13]]],[1,1]]", "[[[[0,7],4],[[7,8],[0,13]]],[1,1]]", 8)]
        [InlineData("[[[[0,7],4],[[7,8],[0,13]]],[1,1]]", "[[[[0,7],4],[[7,8],[0,[6,7]]]],[1,1]]", 9)]
        public void SplitExamples(string input, string expectedNumber, int expectedPairCount)
        {
            var a = SnailfishNumber.Parse(input);
            a.Reduce(1);
            a.ToString().Should().Be(expectedNumber);
            a.GetPairs().Count().Should().Be(expectedPairCount);
        }

        [Fact]
        public void FinalExampleSingleStepReduce()
        {
            var a = SnailfishNumber.Parse("[[[[4,3],4],4],[7,[[8,4],9]]]");
            var b = SnailfishNumber.Parse("[1,1]");

            var sum = SnailfishNumber.AddWithoutReduce(a, b);
            sum.ToString().Should().Be("[[[[[4,3],4],4],[7,[[8,4],9]]],[1,1]]");
            sum.Reduce(1);

            sum.ToString().Should().Be("[[[[0,7],4],[7,[[8,4],9]]],[1,1]]");
            sum.Reduce(1);

            sum.ToString().Should().Be("[[[[0,7],4],[15,[0,13]]],[1,1]]");
            sum.Reduce(1);

            sum.ToString().Should().Be("[[[[0,7],4],[[7,8],[0,13]]],[1,1]]");
            sum.Reduce(1);

            sum.ToString().Should().Be("[[[[0,7],4],[[7,8],[0,[6,7]]]],[1,1]]");
            sum.Reduce(1);

            sum.ToString().Should().Be("[[[[0,7],4],[[7,8],[6,0]]],[8,1]]");
        }
    }
}
