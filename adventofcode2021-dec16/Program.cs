using common;
using common.BitArrayExtensionMethods;
using System.Collections;
using System.Diagnostics;

// todo: read this http://graphics.stanford.edu/~seander/bithacks.html

Console.WriteLine("Day 16: Packet Decoder");


var example1 = "D2FE28";
var p = Packet.Parse(example1);

Debug.Assert(p.Version == 6);
Debug.Assert(p.PacketType == 4);
Debug.Assert(p.Data.ToUInt32() == 2021);
Debug.Assert(p.LengthTypeId == Packet.LengthType.NotSet);


var operatorPacketExample = "38006F45291200";
var op = Packet.Parse(operatorPacketExample);

Debug.Assert(op.Version == 1);
Debug.Assert(op.PacketType == 6);
Debug.Assert(op.LengthTypeId == Packet.LengthType.TotalLengthInBits);

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
    public void SetNibble(byte nibble)
    {
        int offset = PrepareNextOffset();
        var current = Data.Pop();
        current |= nibble << offset;
        Data.Push(current);
        BitLength += 4;
    }

    private int PrepareNextOffset()
    {
        var nibbleOffset = BitLength % 32;
        if (nibbleOffset == 0)
        {
            Data.Push(0);
        }
        return nibbleOffset;
    }

    public void SetBit(bool value)
    {
        int offset = PrepareNextOffset();
        var current = Data.Pop();
        if (value)
        {
            current |= 1 << offset;
        }
        // we don't need an `else` here because this class initializes all the bits to 0.

        Data.Push(current);
        BitLength += 1;
    }

    public uint ToUInt32()
    {
        if (Data.Count > 1)
        {
            throw new ApplicationException($"Can't convert this instance of {nameof(BinaryNumber)} to a uint - it wont fit in 32 bits.");
        }
        uint result = 0;

        var nibbleCount = BitLength / 4;
        var remainingBitCount = BitLength % 4;
        var writeIndex = BitLength - 4;
        for (var readNibbleIndex = 0; readNibbleIndex < nibbleCount; readNibbleIndex++)
        {
            var readMask = 0b1111 << (readNibbleIndex * 4);
            var nibble = Data.Peek() & readMask;

            //it would be interesting to do this without using plus and doing it purely with the bit operators
            result += (uint)(nibble << writeIndex >> (readNibbleIndex * 4));
            writeIndex -= 4;
        }
        if (remainingBitCount > 0)
        {
            result <<= remainingBitCount;
            for (var readBitIndex = 0; readBitIndex < remainingBitCount; readBitIndex++)
            {
                var readMask = 0b1 << BitLength - 1 - readBitIndex;
                var nibble = Data.Peek() & readMask;

                //it would be interesting to do this without using plus and doing it purely with the bit operators
                result += (uint)(nibble << writeIndex >> (readBitIndex * 4));
                writeIndex -= 4;
            }
        }
        return result;
    }

    public int BitLength { get; private set; } = 0;

    public Stack<int> Data { get; private set; } = new(1);
}


public struct Packet
{
    public static Packet Parse(ReadOnlySpan<char> s)
    {
        var ba = new BitArray(ReadBytesFromHexChars(s));
        var result = new Packet();
        
        result.Version = ba.GetByte(VERSION_INDEX, VERSION_LENGTH);        
        result.PacketType = ba.GetByte(PACKET_TYPE_INDEX, PACKET_TYPE_LENGTH);

        if (result.PacketType == PACKET_TYPE_LITERAL_VALUE)
        {
            ParseLiteralPacket();
        }
        else
        {
            ParseOperatorPacket();
        }
        
        return result;

        void ParseOperatorPacket()
        {
            var ltid = ba.GetByte(FIRST_DATA_INDEX, LENGTH_TYPE_ID_LENGTH);
            result.LengthTypeId = ltid == 0b1 ? LengthType.NumberOfSubPackets : LengthType.TotalLengthInBits;

            if (result.LengthTypeId == LengthType.TotalLengthInBits)
            {

            }
            else
            {
                throw new NotImplementedException();
            }
        }

        void ParseLiteralPacket()
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
    }

    public Packet()
    {
        LengthTypeId = LengthType.NotSet;
        Data = new();
        Version = 0;
        PacketType = 0;
    }

    private const int READ_MORE_MASK = 0b10000;
    private const int DATA_MASK = 0b01111;
    private const int VERSION_INDEX = 0;
    private const int VERSION_LENGTH = 3;
    private const int PACKET_TYPE_INDEX = 3;
    private const int PACKET_TYPE_LENGTH = 3;
    private const int LENGTH_TYPE_ID_LENGTH = 1;
    private const int FIRST_DATA_INDEX = 6;
    private const int DATA_LENGTH = 5;
    private const int PACKET_TYPE_LITERAL_VALUE = 4;

    public byte Version { get; private set; }
    public byte PacketType { get; private set; }
    public BinaryNumber Data { get; private set; }
    public LengthType LengthTypeId { get; private set; }
    public enum LengthType
    {
        TotalLengthInBits = 0,
        NumberOfSubPackets = 1,
        NotSet = -1
    }

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