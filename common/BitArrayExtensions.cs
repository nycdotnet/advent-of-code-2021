﻿using System.Collections;
using System.Collections.Specialized;
using System.Numerics;

namespace common.BitArrayExtensionMethods
{
    public static class BitArrayExtensions
    {
        /// <summary>
        /// Tries to convert a string such as "010100010001010010" into a
        /// <see cref="BitArray"/>.  If successful, returns true and sets
        /// the <paramref name="result"/>.  If unsuccessful, returns false
        /// and the <see cref="BitArray"/> will be null.  Should not throw
        /// any exceptions except OutOfMemory exception.
        /// </summary>
        public static bool TryParseToBitArray(this string s, out BitArray? result)
        {
            result = null;
            if (s is null)
            {
                return false;
            }

            var ba = new BitArray(s.Length, false);
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '1':
                        ba.Set(i, true);
                        break;
                    case '0':
                        continue;
                    default:
                        return false;
                }
            }
            result = ba;
            return true;
        }

        public static bool TryParseBigEndianHexToBitArray(this string s, out BitArray? result) =>
            s.AsSpan().TryParseBigEndianHexToBitArray(out result);

        public static bool TryParseBigEndianHexToBitArray(this ReadOnlySpan<char> s, out BitArray? result)
        {
            try
            {
                var bytes = new byte[s.Length / 2];

                var writeIndex = 0;
                for (var charIndex = 0; charIndex < s.Length; charIndex += 2)
                {
                    var a = Utils.CharToNibbleBE(s[charIndex]);
                    var b = Utils.CharToNibbleBE(s[charIndex + 1]);
                    bytes[writeIndex] = (byte)((b << 4) + a);
                    writeIndex++;
                }

                result = new BitArray(bytes);
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Converts <paramref name="i"/> to a <see cref="BitArray"/> of
        /// length <paramref name="length"/>.  If length is not specified,
        /// the length of the result will be 32.  Warning: This does not round trip...
        /// </summary>
        public static BitArray ToBitArray(this int i, int length = 0)
        {
            var ba = new BitArray(new int[] { i });
            if (length == 0)
            {
                return ba;
            }

            var result = new BitArray(length);
            for (var j = 0; j < length; j++)
            {
                // note we have to reverse the order here hence length - j - 1.
                // might be interesting to explore the perf of this versus casting
                // the original BitArray to bool
                result.Set(length - j - 1, ba.Get(j));
            }
            return result;
        }

        /// <summary>
        /// Returns the number of set bits in <paramref name="ba"/>.
        /// Adapted from https://stackoverflow.com/questions/5063178/counting-bits-set-in-a-net-bitarray-class
        /// </summary>
        public static int Cardinality(this BitArray ba)
        {
            var uints = new uint[(ba.Count >> 5) + 1];
            ba.CopyTo(uints, 0);
            var count = 0;
            for (var i = 0; i < uints.Length; i++)
            {
                count += BitOperations.PopCount(uints[i]);
            }
            return count;
        }

        /// <summary>
        /// Tries to convert a string composed of only 0 and 1 such as "010100010001010010" into a
        /// <see cref="BitVector32"/>.  If successful, returns true and sets the
        /// <paramref name="result"/>.  If unsuccessful, returns false and the
        /// <see cref="BitArray"/> will be null.  Will always fail if <paramref name="s"/> is null
        /// or longer than 32 characters.
        /// </summary>
        public static bool TryParseToBitVector32(this string s, out BitVector32? result)
        {
            if (s is null || s.Length > 32)
            {
                result = null;
                return false;
            }
            try
            {
                result = new BitVector32(Convert.ToInt32(s, 2));
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }


        /// <summary>
        /// Reads a byte out of a BitArray.  OK to specify a length less than 8 - will just pad zeros on the byte.
        /// </summary>
        public static byte GetByte(this BitArray ba, int startIndex, int length)
        {
            const int bitSize = sizeof(byte) * 8;
            if (length > bitSize)
            {
                throw new ArgumentOutOfRangeException($"Can't read more than {bitSize} bits into a byte.");
            }
            byte result = 0;
            var writeIndex = length - 1;
            for (var readIndex = startIndex; readIndex < length + startIndex; readIndex++)
            {
                if (ba.Get(readIndex))
                {
                    // this sets the bit at the writeIndex
                    result |= (byte)(1 << writeIndex);
                }
                writeIndex--;
            }
            return result;
        }


        /// <summary>
        /// Formats the contents of a bit array as a string of characters
        /// in index order, i.e. the index 0 of the BitArray will be the first
        /// character in the returned string.  False bits are formatted as
        /// <paramref name="falseCharacter"/>, which defaults to '0', and true
        /// bits are formatted as <paramref name="trueCharacter"/>, which defaults
        /// to '1'.
        /// </summary>
        /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/csharp/how-to/modify-string-contents#programmatically-build-up-string-content"/>
        public static string Format(this BitArray ba, char falseCharacter = '0', char trueCharacter = '1') =>
            // NOTE: I think the below may be wrong because it creates a closure.  I think you're supposed to use the incoming char[].
            string.Create(ba.Length, new char[ba.Length], (Span<char> result, char[] _) => {
                for (int i = 0; i < ba.Length; i++)
                {
                    result[i] = ba.Get(i) ? trueCharacter : falseCharacter;
                }
            });

        /// <summary>
        /// Converts the bits in <paramref name="ba"/> to a <see cref="uint"/> in a manner where the last bit
        /// in the <paramref name="ba"/> is the least significant.  If there are more than 32 bits in
        /// <paramref name="ba"/>, will throw.  (NOTE: This is opposite the way that numbers typically work in .NET)
        /// </summary>
        /// <remarks>
        /// Adapted from Marc Gravel's answer at https://stackoverflow.com/questions/713057/convert-bool-to-byte
        /// </remarks>
        public static uint GetBigEndianUInt32(this BitArray ba)
        {
            if (ba.Length > 32)
            {
                throw new ArgumentException(message: $"The {nameof(GetBigEndianUInt32)} extension method does not support {nameof(BitArray)} parameters with more than 32 bits.", paramName: nameof(ba));
            }
            return GetBigEndianUInt32(ba, 0, ba.Length);
        }

        public static ulong GetBigEndianULong(this BitArray ba)
        {
            if (ba.Length > 64)
            {
                throw new ArgumentException(message: $"The {nameof(GetBigEndianULong)} extension method does not support {nameof(BitArray)} parameters with more than 64 bits.", paramName: nameof(ba));
            }
            return GetBigEndianULong(ba, 0, ba.Length);
        }

        /// <summary>
        /// Converts the bits in <paramref name="ba"/> to a <see cref="uint"/> in a manner where the last bit
        /// in the <paramref name="ba"/> is the least significant.  If there are more than 32 bits in
        /// <paramref name="ba"/>, will throw.  (NOTE: This is opposite the way that numbers typically work in .NET)
        /// </summary>
        /// <remarks>
        /// Adapted from Marc Gravel's answer at https://stackoverflow.com/questions/713057/convert-bool-to-byte
        /// </remarks>
        public static uint GetBigEndianUInt32(this BitArray ba, int startIndex, int length)
        {
            if (length > 32)
            {
                throw new ArgumentException(message: $"The {nameof(GetBigEndianUInt32)} extension method does not support getting a length of more than 32 bits.", paramName: nameof(ba));
            }
            var result = 0u;
            var bit = 1u;
            for (var i = startIndex + length - 1; i >= startIndex; i--)
            {
                if (ba.Get(i))
                {
                    result |= bit;
                }
                bit <<= 1;
            }
            return result;
        }

        public static ulong GetBigEndianULong(this BitArray ba, int startIndex, int length)
        {
            if (length > 64)
            {
                throw new ArgumentException(message: $"The {nameof(GetBigEndianULong)} extension method does not support getting a length of more than 64 bits.", paramName: nameof(ba));
            }
            var result = 0ul;
            var bit = 1ul;
            for (var i = startIndex + length - 1; i >= startIndex; i--)
            {
                if (ba.Get(i))
                {
                    result |= bit;
                }
                bit <<= 1;
            }
            return result;
        }



        /// <summary>
        /// Returns a new <see cref="BitArray"/> with the content of <paramref name="ba"/> starting from
        /// <paramref name="startIndex"/> to the end.  Does not mutate <paramref name="ba"/>.
        /// </summary>
        public static BitArray Slice(this BitArray ba, int startIndex)
        {
            var length = ba.Length - startIndex;
            return ba.Slice(startIndex, length);
        }


        /// <summary>
        /// Returns a new <see cref="BitArray"/> with the content of <paramref name="ba"/> starting from
        /// <paramref name="startIndex"/> with length <paramref name="length"/>.  Does not mutate <paramref name="ba"/>.
        /// </summary>
        public static BitArray Slice(this BitArray ba, int startIndex, int length)
        {
            var result = new BitArray(length);
            for (var i = 0; i < length; i++)
            {
                result.Set(i, ba.Get(i + startIndex));
            }
            return result;
        }

        /// <summary>
        /// Mutates a <see cref="BitArray"/> by shifting its bits left by <paramref name="count"/> places.
        /// Overflow bits are discarded.  Underflow bits are set to 0.  Runs in linear time based on the size
        /// of the <see cref="BitArray"/>, but operates on one bit at a time so I'm fairly certain this could be
        /// optimized.  NOTE: This is #1 not needed (it's actually in the framework since .NET 6) and #2 apparently
        /// the opposite of how the framework version works!!
        /// </summary>
        public static void ShiftLeftBE(this BitArray ba, int count)
        {
            if (count == 0)
            {
                return;
            }
            if (count < 0)
            {
                throw new NotSupportedException("Can't use this method to shift left a negative amount.  TODO: make a shift right.");
            }
            if (count >= ba.Length)
            {
                // since this will overflow all the bits, we just set the underflow to 0 and return.
                ba.SetAll(false);
                return;
            }

            for (var i = 0; i < ba.Length - count; i++)
            {
                ba.Set(i, ba.Get(i + count));
            }
            for (var i = ba.Length - count; i < ba.Length; i++)
            {
                ba.Set(i, false);
            }
        }
    }
}
