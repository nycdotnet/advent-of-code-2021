using common.BitArrayExtensionMethods;
using FluentAssertions;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Xunit;

namespace tests
{
    public class BinaryParseExtensionMethodTests
    {
        [Theory]
        [InlineData("00100", "~~*~~")]
        [InlineData("100010011000", "*~~~*~~**~~~")]
        [InlineData("", "")]
        public void CanRoundTripBinaryParseAndFormat(string input, string expected)
        {
            var isParseable = input.TryParseToBitArray(out var ba);
            isParseable.Should().BeTrue();
            ba.Should().NotBeNull();
            ba!.Length.Should().Be(input.Length);

            // makes it look festive, like garland or something.
            ba.Format('~', '*').Should().Be(expected);
        }

        [Theory]
        [InlineData("D2FE28", "110100101111111000101000")]
        [InlineData("38006F45291200", "00111000000000000110111101000101001010010001001000000000")]
        public void CanRoundTrip_TryParseBigEndianHexToBitArray(string input, string expected)
        {
            var isParseable = input.TryParseBigEndianHexToBitArray(out var ba);
            isParseable.Should().BeTrue();
            ba.Should().NotBeNull();
            ba.Format().Should().Be(expected);
        }

        [Theory]
        [InlineData("00100", "~~*~~")]
        [InlineData("100010011000", "*~~~*~~**~~~")]
        public void CanRoundTripBinaryParseAndFormatUsingSimplerMethods(string input, string expected)
        {
            var parsed = Convert.ToInt32(input, 2);
            var ba = parsed.ToBitArray(input.Length);
            ba.Should().NotBeNull();

            // makes it look festive, like garland or something.
            ba.Format('~', '*').Should().Be(expected);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("Happy Holidays!")]
        [InlineData(null)]
        public void StringsThatCannotBeParsedReturnFalseAndTheOutBitArrayWillBeNull(string input)
        {
            var isParseable = input.TryParseToBitArray(out var ba);
            isParseable.Should().BeFalse();
            ba.Should().BeNull();
        }

        [Theory]
        [InlineData("10110", 22u)]
        [InlineData("01001", 9u)]
        [InlineData("11111111111111111111111111111111", uint.MaxValue)]
        [InlineData("11111111111111111111111111111110", 4294967294u)]
        [InlineData("01111111111111111111111111111111", 2147483647u)]
        [InlineData("1111111111111111111111111111111", 2147483647u)]
        [InlineData("", 0)]
        public void CanGetBigEndianUInt32(string input, uint expected)
        {
            var isParseable = input.TryParseToBitArray(out var ba);
            isParseable.Should().BeTrue();
            ba.Should().NotBeNull();
            ba!.Length.Should().Be(input.Length);
            ba.GetBigEndianUInt32().Should().Be(expected);
        }

        [Theory]
        [InlineData("10101", 4, 1, 1u)]
        [InlineData("10101", 3, 1, 0u)]
        [InlineData("10101", 0, 3, 5u)]
        [InlineData("10101", 0, 5, 21u)]
        [InlineData("", 0, 0, 0u)]
        public void CanGetBigEndianUInt32WithSlicing(string input, int startIndex, int length, uint expected)
        {
            var isParseable = input.TryParseToBitArray(out var ba);
            isParseable.Should().BeTrue();
            ba.Should().NotBeNull();
            ba!.Length.Should().Be(input.Length);
            ba.GetBigEndianUInt32(startIndex, length).Should().Be(expected);
        }

        // see: https://github.com/dotnet/runtime/issues/73094
        [Fact]
        public void Issue73094_ba2h()
        {
            var source = new byte[] { 0x7F, 0x00, 0xFF, 0x00, 0xFF };
            var b = new BitArray(source);
            
            Assert.Equal("7F00FF00FF", BitArrayToHex2(b));

            string BitArrayToHex2(BitArray bits)
            {
                if (bits.Length % 8 != 0)
                {
                    throw new NotSupportedException("This method only supports BitArrays with a length evenly divisible by 8.");
                }
                var sb = new StringBuilder(bits.Length / 4);

                // NOTE: I think this only works if the length is evenly divisible by 8.
                for (int i = 0; i < bits.Length; i += 8)
                {
                    int v = (bits[i + 7] ? 128 : 0) |
                        (bits[i + 6] ? 64 : 0) |
                        (bits[i + 5] ? 32 : 0) |
                        (bits[i + 4] ? 16 : 0) |
                        (bits[i + 3] ? 8 : 0) |
                        (bits[i + 2] ? 4 : 0) |
                        (bits[i + 1] ? 2 : 0) |
                        (bits[i + 0] ? 1 : 0);

                    _ = sb.Append(v.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        [Fact]
        public void Issue73094Small()
        {
            var source = new byte[] { 0x7F, 0x00, 0xFF, 0x00, 0xFF };
            var b = new BitArray(source);

            var initialString = formatBitArraySimple(b);

            var result = b.LeftShift(3);

            var expectedString = "000" + initialString.Substring(0, initialString.Length - 3);

            Assert.Equal(expectedString, formatBitArraySimple(b));
            Assert.Equal(expectedString, formatBitArraySimple(result));
        }

        [Fact]
        public void Issue73094()
        {
            var x = 1;
            x <<= 1;
            Assert.Equal(2, x);

            var baExample = new BitArray(3);
            baExample.Set(1, true);
            baExample.RightShift(1);
            var rs = formatBitArraySimple(baExample);


            var source = new byte[] { 0x7F, 0x00, 0xFF, 0x00, 0xFF };

            // we want to shift from zero to the length in bits, hence Length * 8 as the max
            for (var i = 0; i <= source.Length * 8; i++)
            {
                var b = new BitArray(source);
                var initialString = formatBitArraySimple(b);

                var result = b.LeftShift(i);

                var expectedString = new string('0', i) + initialString.Substring(0, initialString.Length - i);

                Assert.Equal(expectedString, formatBitArraySimple(b));
                Assert.Equal(expectedString, formatBitArraySimple(result));
                Assert.Equal(40, expectedString.Length);
            }
        }

        private string formatBitArraySimple(BitArray ba)
        {
            var sb = new StringBuilder(ba.Length);
            for (var i = 0; i < ba.Length; i++)
            {
                sb.Append(ba.Get(i) ? '1' : '0');
            }
            return sb.ToString();
        }

        [Fact]
        public void ShiftLeftSetsUnderflowToZero()
        {
            var ba = new BitArray(64);
            ba.SetAll(true);
            ba.GetBigEndianULong().Should().Be(ulong.MaxValue);
            ba.ShiftLeftBE(1);
            ba.Format().Should().Be("1111111111111111111111111111111111111111111111111111111111111110");
            ba.GetBigEndianULong().Should().Be(ulong.MaxValue - 1);
        }

        [Fact]
        public void ShiftLeftWhenOverShiftedReturnsAllZeros()
        {
            var ba = new BitArray(64);
            ba.SetAll(true);
            ba.GetBigEndianULong().Should().Be(ulong.MaxValue);
            ba.ShiftLeftBE(64);
            ba.GetBigEndianULong().Should().Be(0);
        }

        [Theory]
        [InlineData("10110", 22)]
        [InlineData("01001", 9)]
        [InlineData("11111111111111111111111111111111", -1)]
        [InlineData("00000000000000000000000000000000", 0)]
        [InlineData("10000000000000000000000000000000", int.MinValue)]
        [InlineData("1111111111111111111111111111111", int.MaxValue)]  // 31 '1'.
        [InlineData("01111111111111111111111111111111", int.MaxValue)] // 31 '1' with a leading '0'
        public void AssertingOnHowSystemConvertToInt32Works(string input, int expected)
        {
            var result = Convert.ToInt32(input, 2);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, 32)]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 2)]
        public void BitArrayCardinalityExtensionWorksAsExpected(int bitArraySeedValue, int expectedCardinality)
        {
            var ba = new BitArray(new int[] { bitArraySeedValue });
            ba.Cardinality().Should().Be(expectedCardinality);
        }

        [Theory]
        [InlineData(1, 1, 0, 0, 0)]
        [InlineData(int.MaxValue, 255, 255, 255, 127)]
        [InlineData(-2, 254, 255, 255, 255)]
        [InlineData(-1, 255, 255, 255, 255)]
        public void AssertingOnHowBitVector32Works(int initial, int expectedA, int expectedB, int expectedC, int expectedD)
        {
            // This is an interesing API for packing a bunch of bits and tiny uints into a single int32.
            // It's super optimized for cramming/using less RAM, but has way more setup than BitArray
            // due to all the up-front work you need to do defining the Sections or Masks.
            // https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.bitvector32.createsection?view=net-6.0

            // This example sets up four 8 bit uints
            var sectionA = BitVector32.CreateSection(255);
            var sectionB = BitVector32.CreateSection(255, sectionA);
            var sectionC = BitVector32.CreateSection(255, sectionB);
            var sectionD = BitVector32.CreateSection(255, sectionC);

            var bv = new BitVector32(initial);
            bv[sectionA].Should().Be(expectedA);
            bv[sectionB].Should().Be(expectedB);
            bv[sectionC].Should().Be(expectedC);
            bv[sectionD].Should().Be(expectedD);
        }

        [Fact]
        public void BitVector32CanSupportEightNibblesWithArrayOfSections()
        {
            // the nibble sections after the first must reference the previous ones.
            // these sections are "static" in the sense that they don't reference
            // any particular instance of a BitVector32.  In this example I am
            // using an array to store them.
            var nibbles = new BitVector32.Section[BitsInAnInt / BitsInANibble];
            for (var i = 0; i < nibbles.Length; i++)
            {
                if (i == 0)
                {
                    // the first section just references the max value we want to be able to store.
                    nibbles[i] = BitVector32.CreateSection(NibbleMaxValue);
                }
                else
                {
                    // the successive sections reference the max value we want to store AND the previous section
                    nibbles[i] = BitVector32.CreateSection(NibbleMaxValue, nibbles[i - 1]);
                }
            }

            var bv = new BitVector32(0);
            for (var i = 0; i < nibbles.Length; i++)
            {
                // let's set each nibble to an integer value of 1, 2, 3, etc.
                bv[nibbles[i]] = i + 1;
            }

            for (var i = 0; i < nibbles.Length; i++)
            {
                bv[nibbles[i]].Should().Be(i + 1);
            }

            // BitVector32 has nice debugging support via an overload to ToString()
            bv.ToString().Should().Be("BitVector32{10000111011001010100001100100001}");
        }

        [Fact]
        public void BitVector32CanSupportEightNibblesInIndividuallyDefinedSections()
        {
            // the nibble sections after the first must reference the previous ones.
            // these sections are "static" in the sense that they don't reference
            // any particular instance of a BitVector32.
            var nibbleA = BitVector32.CreateSection(NibbleMaxValue);
            var nibbleB = BitVector32.CreateSection(NibbleMaxValue, nibbleA);
            var nibbleC = BitVector32.CreateSection(NibbleMaxValue, nibbleB);
            var nibbleD = BitVector32.CreateSection(NibbleMaxValue, nibbleC);
            var nibbleE = BitVector32.CreateSection(NibbleMaxValue, nibbleD);
            var nibbleF = BitVector32.CreateSection(NibbleMaxValue, nibbleE);
            var nibbleG = BitVector32.CreateSection(NibbleMaxValue, nibbleF);
            var nibbleH = BitVector32.CreateSection(NibbleMaxValue, nibbleG);

            var bv = new BitVector32(0);
            bv[nibbleA] = 1;
            bv[nibbleB] = 2;
            bv[nibbleC] = 3;
            bv[nibbleD] = 4;
            bv[nibbleE] = 5;
            bv[nibbleF] = 6;
            bv[nibbleG] = 7;
            bv[nibbleH] = 8;

            bv[nibbleA].Should().Be(1);
            bv[nibbleB].Should().Be(2);
            bv[nibbleC].Should().Be(3);
            bv[nibbleD].Should().Be(4);
            bv[nibbleE].Should().Be(5);
            bv[nibbleF].Should().Be(6);
            bv[nibbleG].Should().Be(7);
            bv[nibbleH].Should().Be(8);

            // BitVector32 has nice debugging support via an overload to ToString()
            bv.ToString().Should().Be("BitVector32{10000111011001010100001100100001}");
        }

        const short BitsInANibble = 4;
        const short NibbleMaxValue = 8;
        const short BitsInAnInt = 32;
    }
}