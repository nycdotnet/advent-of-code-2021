using common;
using common.BitArrayExtensionMethods;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// todo: read this http://graphics.stanford.edu/~seander/bithacks.html

Console.WriteLine("Day 16: Packet Decoder");


var example1 = "D2FE28";
var p = Packet.Parse(example1);

Debug.Assert(p.Version == 6);
Debug.Assert(p.PacketType == 4);
Debug.Assert(p.Data.GetBigEndianUInt32() == 2021);
Debug.Assert(p.LengthTypeId == Packet.LengthType.NotSet);


var operatorPacketExample = "38006F45291200";
var op = Packet.Parse(operatorPacketExample);

Debug.Assert(op.Version == 1);
Debug.Assert(op.PacketType == 6);
Debug.Assert(op.LengthTypeId == Packet.LengthType.TotalLengthInBits);
Debug.Assert(op.SubPackets.Count == 2);
Debug.Assert(op.SubPackets[0].Data.GetBigEndianUInt32() == 10);
Debug.Assert(op.SubPackets[1].Data.GetBigEndianUInt32() == 20);

var operatorPacketExample2 = "EE00D40C823060";
var op2 = Packet.Parse(operatorPacketExample2);

Debug.Assert(op2.PacketType == 3);
Debug.Assert(op2.Version == 7);
Debug.Assert(op2.LengthTypeId == Packet.LengthType.NumberOfSubPackets);
Debug.Assert(op2.DeclaredCountOfSubPackets == 3);
Debug.Assert(op2.SubPackets.Count == 3);

Debug.Assert(op2.SubPackets[0].Data.GetBigEndianUInt32() == 1);
Debug.Assert(op2.SubPackets[1].Data.GetBigEndianUInt32() == 2);
Debug.Assert(op2.SubPackets[2].Data.GetBigEndianUInt32() == 3);



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
    // todo: add growing.
    public BinaryNumber()
    {
        Data = new(32);
        BitLength = 0;
    }
    public BitArray Data { get; private set; }
    public int BitLength { get; private set; }
    // Will set the four least significant bits to the four least significant bits of the passed byte.
    // If any data has already been set, the existing data will be pushed to be more significant by four
    // bits.
    public void PushNibble(byte nibble)
    {
        if (BitLength > 0)
        {
            Data.ShiftLeft(4);
        }
        var setIndex = 31;
        Data.Set(setIndex--, (nibble & 1) == 1);
        Data.Set(setIndex--, (nibble & 2) == 2);
        Data.Set(setIndex--, (nibble & 4) == 4);
        Data.Set(setIndex--, (nibble & 8) == 8);
        BitLength += 4;
    }
    public uint GetBigEndianUInt32() => Data.GetBigEndianUInt32();
}



public struct Packet
{
    public static Packet Parse(ReadOnlySpan<char> s)
    {
        //todo: throw if this is not possible.
        _ = s.TryParseBigEndianHexToBitArray(out var ba);
        var result = ParseStartingAt(ba, 0);
        return result.packet;
    }

    public static (Packet packet, int bitsRead) ParseStartingAt(BitArray ba, int startIndex)
    {
        var result = new Packet();
        
        result.Version = (byte)ba.GetBigEndianUInt32(VERSION_INDEX + startIndex, VERSION_LENGTH);        
        result.PacketType = (byte)ba.GetBigEndianUInt32(PACKET_TYPE_INDEX + startIndex, PACKET_TYPE_LENGTH);

        int bitsRead = VERSION_LENGTH + PACKET_TYPE_LENGTH;

        if (result.PacketType == PACKET_TYPE_LITERAL_VALUE)
        {
            ParseLiteralPacket();
        }
        else
        {
            ParseOperatorPacket();
        }
        
        return (packet: result, bitsRead: bitsRead);

        void ParseOperatorPacket()
        {
            // todo: fix everything in here to just use bitsRead.
            var ltid = ba.GetBigEndianUInt32(FIRST_DATA_INDEX + startIndex, LENGTH_TYPE_ID_LENGTH);
            result.LengthTypeId = ltid == 0b1 ? LengthType.NumberOfSubPackets : LengthType.TotalLengthInBits;
            bitsRead += 1;

            if (result.LengthTypeId == LengthType.TotalLengthInBits)
            {
                // the length will be a 15 bit number (in the next 15 bits)
                var subPacketBitLength = ba.GetBigEndianUInt32(FIRST_DATA_INDEX + startIndex + 1, 15);
                bitsRead += 15 + Convert.ToInt32(subPacketBitLength);
                var subPacketBits = ba.Slice(FIRST_DATA_INDEX + startIndex + 1 + 15, Convert.ToInt32(subPacketBitLength));
                Debug.Assert(subPacketBits.Length == 27);
                var subPacketBitsRead = 0;
                while (subPacketBitsRead < subPacketBitLength)
                {
                    var subPacketParseResult = ParseStartingAt(subPacketBits, subPacketBitsRead);
                    result.SubPackets.Add(subPacketParseResult.packet);
                    subPacketBitsRead += subPacketParseResult.bitsRead;
                }
            }
            else
            {
                // the count of subpackets will be a 11 bit number (in the next 11 bits)
                result.DeclaredCountOfSubPackets = Convert.ToInt32(ba.GetBigEndianUInt32(FIRST_DATA_INDEX + startIndex + 1, 11));
                bitsRead += 11;

                while (result.SubPackets.Count < result.DeclaredCountOfSubPackets)
                {
                    var potentialSubPacketBits = ba.Slice(bitsRead);
                    var subPacketParseResult = ParseStartingAt(potentialSubPacketBits, 0);
                    bitsRead += subPacketParseResult.bitsRead;
                    result.SubPackets.Add(subPacketParseResult.packet);
                }
            }
        }

        void ParseLiteralPacket()
        {
            var readIndex = FIRST_DATA_INDEX + startIndex;
            var readMore = true;

            while (readMore)
            {
                var next = ba.GetByte(readIndex, 5);
                bitsRead += 5;
                var nibble = (byte)(next & DATA_MASK);
                
                result.Data.PushNibble(nibble);

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
        DeclaredCountOfSubPackets = 0;
        SubPackets = new();
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
    public List<Packet> SubPackets { get; private set; }
    public int DeclaredCountOfSubPackets { get; private set; }
    public enum LengthType
    {
        TotalLengthInBits = 0,
        NumberOfSubPackets = 1,
        NotSet = -1
    }
}