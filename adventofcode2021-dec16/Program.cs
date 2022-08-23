using common;
using common.BitArrayExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using static System.Collections.Specialized.BitVector32;

Console.WriteLine("Day 16: Packet Decoder");






var example1 = "D2FE28";
var p = Packet.Parse(example1);

Debug.Assert(p.Version == 6);
Debug.Assert(p.PacketType == 4);
Debug.Assert(p.Data.ToUInt32() == 2021);

//var first = p.Data[0].ToString("X");
//var second = p.Data[1].ToString("X");

//var expected = 2021;
//var expectedBA = expected.ToBitArray();
//Console.WriteLine($"the expected BA is {expectedBA.Format()}");
//// the expected BA is 10100111111000000000000000000000
//var expectedRT = expectedBA.ToUInt32();
//Debug.Assert(expectedRT == 2021);

//var ba = new BitArray(p.Data);
//Console.WriteLine($"data in packet is {ba.Format()}");
//Debug.Assert(ba.Format() == "011111100101");


public class BinaryNumber
{
    public BinaryNumber()
    {
    }

    // For reference - I have validated that the nibbles are coming in correctly.
    public void SetNibble(byte nibble)
    {
        var nibbleOffset = BitLength % 32;
        if (nibbleOffset == 0)
        {
            Data.Push(0);
        }
        var value = Data.Pop();
        value |= nibble << nibbleOffset;
        Data.Push(value);

        BitLength += 4;
    }

    public uint ToUInt32()
    {
        if (Data.Count > 1)
        {
            throw new ApplicationException($"Can't convert this instance of {nameof(BinaryNumber)} to a uint - it wont fit in 32 bits.");
        }
        uint result = 0;

        var nibbleCount = BitLength / 4;
        var writeIndex = BitLength - 4;
        for (var readNibbleIndex = 0; readNibbleIndex < nibbleCount; readNibbleIndex++)
        {
            var readMask = 0b1111 << (readNibbleIndex * 4);
            var nibble = Data.Peek() & readMask;

            //it would be interesting to do this without using plus and doing it purely with the bit operators
            result += (uint)(nibble << writeIndex >> (readNibbleIndex * 4));
            writeIndex -= 4;
        }
        return result;
    }

    public int BitLength { get; private set; } = 0;

    public Stack<int> Data { get; private set; } = new(1);
}


public struct Packet
{
    //The BITS transmission contains a single packet at its outermost layer which itself contains
    // many other packets. The hexadecimal representation of this packet might encode a few extra 0
    // bits at the end; these are not part of the transmission and should be ignored.
    // Every packet begins with a standard header: the first three bits encode the packet version,
    // and the next three bits encode the packet type ID.These two values are numbers; all numbers
    // encoded in any packet are represented as binary with the most significant bit first.
    // For example, a version encoded as the binary sequence 100 represents the number 4.
    // Packets with type ID 4 represent a literal value.  Literal value packets encode a single binary
    // number.To do this, the binary number is padded with leading zeroes until its length is a multiple
    // of four bits, and then it is broken into groups of four bits. Each group is prefixed by a 1 bit
    // except the last group, which is prefixed by a 0 bit. These groups of five bits immediately
    // follow the packet header.
    public static Packet Parse(ReadOnlySpan<char> s)
    {
        var ba = new BitArray(ReadBytesFromHexChars(s));
        var result = new Packet();
        
        result.Version = ba.GetByte(VERSION_INDEX, VERSION_LENGTH);        
        result.PacketType = ba.GetByte(PACKET_TYPE_INDEX, PACKET_TYPE_LENGTH);
        result.Data = new();

        if (result.PacketType == PACKET_TYPE_LITERAL_VALUE)
        {
            var readIndex = FIRST_DATA_INDEX;
            var readMore = true;

            while (readMore)
            {
                var next = ba.GetByte(readIndex, 5);
                var nibble = (byte)(next & DATA_MASK);
                result.Data.SetNibble(nibble);

                // binary AND the READ_MORE_MASK to next and then shift the result 4 spots right, so a 16 becomes a 1.
                readMore = ((next & READ_MORE_MASK) >> 4) == 1;
                readIndex += DATA_LENGTH;
            }
        }
        
        return result;
    }

    private const int READ_MORE_MASK = 0b10000;
    private const int DATA_MASK = 0b01111;
    private const int VERSION_INDEX = 0;
    private const int VERSION_LENGTH = 3;
    private const int PACKET_TYPE_INDEX = 3;
    private const int PACKET_TYPE_LENGTH = 3;
    private const int FIRST_DATA_INDEX = 6;
    private const int DATA_LENGTH = 5;
    private const int PACKET_TYPE_LITERAL_VALUE = 4;

    public byte Version { get; private set; }
    public byte PacketType { get; private set; }
    public BinaryNumber Data { get; private set; }

    private static byte[] ReadBytesFromHexChars(ReadOnlySpan<char> s)
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

        return bytes;
    }
}