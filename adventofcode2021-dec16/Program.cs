using common.BitArrayExtensionMethods;
using System.Collections;
using System.Diagnostics;
using static common.Utils;

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

var moreExample1Hex = "8A004A801A8002F478";
var me1 = Packet.Parse(moreExample1Hex);
Debug.Assert(me1.SumOfVersionsWithChildren() == 16);

var moreExample2Hex = "620080001611562C8802118E34";
var me2 = Packet.Parse(moreExample2Hex);
Debug.Assert(me2.SumOfVersionsWithChildren() == 12);

var moreExample3Hex = "C0015000016115A2E0802F182340";
var me3 = Packet.Parse(moreExample3Hex);
Debug.Assert(me3.SumOfVersionsWithChildren() == 23);

var moreExample4Hex = "A0016C880162017C3686B18A3D4780";
var me4 = Packet.Parse(moreExample4Hex);
Debug.Assert(me4.SumOfVersionsWithChildren() == 31);


RunPart1();


void RunPart1()
{
    var hex = GetLines("myPuzzleInput.txt").First();
    var outerPacket = Packet.Parse(hex);
    Console.WriteLine($"Part one answer: {outerPacket.SumOfVersionsWithChildren()}");
}


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
        int bitsRead = 0;

        result.Version = (byte)ba.GetBigEndianUInt32(bitsRead + startIndex, VERSION_LENGTH);
        bitsRead += VERSION_LENGTH;

        result.PacketType = (byte)ba.GetBigEndianUInt32(bitsRead + startIndex, PACKET_TYPE_LENGTH);
        bitsRead += PACKET_TYPE_LENGTH;

        if (result.IsLiteralValue)
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
            var ltid = ba.GetBigEndianUInt32(bitsRead + startIndex, LENGTH_TYPE_ID_LENGTH);
            result.LengthTypeId = ltid == 0b1 ? LengthType.NumberOfSubPackets : LengthType.TotalLengthInBits;
            bitsRead += 1;

            if (result.LengthTypeId == LengthType.TotalLengthInBits)
            {
                // the length will be a 15 bit number (in the next 15 bits)
                var subPacketBitLength = ba.GetBigEndianUInt32(bitsRead + startIndex, SUBPACKETS_BIT_LENGTH_BITS_SIZE);
                bitsRead += SUBPACKETS_BIT_LENGTH_BITS_SIZE;

                var subPacketBits = ba.Slice(bitsRead + startIndex, Convert.ToInt32(subPacketBitLength));
                var subPacketBitsRead = 0;
                while (subPacketBitsRead < subPacketBitLength)
                {
                    var subPacketParseResult = ParseStartingAt(subPacketBits, subPacketBitsRead);
                    result.SubPackets.Add(subPacketParseResult.packet);
                    subPacketBitsRead += subPacketParseResult.bitsRead;
                    bitsRead += subPacketParseResult.bitsRead;
                }
            }
            else
            {
                // the count of subpackets will be a 11 bit number (in the next 11 bits)
                result.DeclaredCountOfSubPackets = Convert.ToInt32(ba.GetBigEndianUInt32(bitsRead + startIndex, COUNT_OF_SUBPACKETS_BITS_SIZE));
                bitsRead += COUNT_OF_SUBPACKETS_BITS_SIZE;

                while (result.SubPackets.Count < result.DeclaredCountOfSubPackets)
                {
                    var potentialSubPacketBits = ba.Slice(bitsRead + startIndex);
                    var subPacketParseResult = ParseStartingAt(potentialSubPacketBits, 0);
                    bitsRead += subPacketParseResult.bitsRead;
                    result.SubPackets.Add(subPacketParseResult.packet);
                }
            }
        }

        void ParseLiteralPacket()
        {
            var readIndex = bitsRead + startIndex;
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

    public int SumOfVersionsWithChildren() =>
        Convert.ToInt32(Version) + SubPackets.Sum(p => p.SumOfVersionsWithChildren());

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
    private const int VERSION_LENGTH = 3;
    private const int PACKET_TYPE_LENGTH = 3;
    private const int LENGTH_TYPE_ID_LENGTH = 1;
    private const int DATA_LENGTH = 5;
    private const int PACKET_TYPE_LITERAL_VALUE = 4;
    private const int COUNT_OF_SUBPACKETS_BITS_SIZE = 11;
    private const int SUBPACKETS_BIT_LENGTH_BITS_SIZE = 15;

    public bool IsLiteralValue => PacketType == PACKET_TYPE_LITERAL_VALUE;
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