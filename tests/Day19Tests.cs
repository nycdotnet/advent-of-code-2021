using adventofcode2021_dec19;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace tests
{
    public class Day19Tests
    {
        [Fact]
        public void InitialParseWorks()
        {
            var scanners = Scanner.ParseInput(TwoDimensionalInput.Replace("\r", "").Split('\n'));
            scanners.Should().HaveCount(2);
            scanners[0].Id.Should().Be(0);
            scanners[0].Points.Should().HaveCount(3);
            scanners[0].Points[0].X.Should().Be(0);
            scanners[0].Points[0].Y.Should().Be(2);
            scanners[0].Points[1].X.Should().Be(4);
            scanners[0].Points[1].Y.Should().Be(1);
            scanners[0].Points[2].X.Should().Be(3);
            scanners[0].Points[2].Y.Should().Be(3);

            scanners[1].Id.Should().Be(1);
            scanners[1].Points[0].X.Should().Be(-1);
            scanners[1].Points[0].Y.Should().Be(-1);
            scanners[1].Points[1].X.Should().Be(-5);
            scanners[1].Points[1].Y.Should().Be(0);
            scanners[1].Points[2].X.Should().Be(-2);
            scanners[1].Points[2].Y.Should().Be(1);
        }

        public static readonly string TwoDimensionalInput = @"
--- scanner 0 ---
0,2
4,1
3,3

--- scanner 1 ---
-1,-1
-5,0
-2,1".Trim();
    }
}
