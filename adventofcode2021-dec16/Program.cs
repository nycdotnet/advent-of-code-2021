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
Debug.Assert(p.Data.GetBigEndianULong() == 2021);
Debug.Assert(p.LengthTypeId == Packet.LengthType.NotSet);

var operatorPacketExample = "38006F45291200";
var op = Packet.Parse(operatorPacketExample);

Debug.Assert(op.Version == 1);
Debug.Assert(op.PacketType == 6);
Debug.Assert(op.LengthTypeId == Packet.LengthType.TotalLengthInBits);
Debug.Assert(op.SubPackets.Count == 2);
Debug.Assert(op.SubPackets[0].Data.GetBigEndianULong() == 10);
Debug.Assert(op.SubPackets[1].Data.GetBigEndianULong() == 20);

var operatorPacketExample2 = "EE00D40C823060";
var op2 = Packet.Parse(operatorPacketExample2);

Debug.Assert(op2.PacketType == 3);
Debug.Assert(op2.Version == 7);
Debug.Assert(op2.LengthTypeId == Packet.LengthType.NumberOfSubPackets);
Debug.Assert(op2.DeclaredCountOfSubPackets == 3);
Debug.Assert(op2.SubPackets.Count == 3);

Debug.Assert(op2.SubPackets[0].Data.GetBigEndianULong() == 1);
Debug.Assert(op2.SubPackets[1].Data.GetBigEndianULong() == 2);
Debug.Assert(op2.SubPackets[2].Data.GetBigEndianULong() == 3);

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

var p2e1 = Packet.Parse("C200B40A82");
Debug.Assert(p2e1.Value() == 3);
Console.WriteLine(p2e1.AST());

var p2e2 = Packet.Parse("04005AC33890");
Debug.Assert(p2e2.Value() == 54);
Console.WriteLine(p2e2.AST());

var p2e3 = Packet.Parse("880086C3E88112");
Debug.Assert(p2e3.Value() == 7);
Console.WriteLine(p2e3.AST());

var p2e4 = Packet.Parse("CE00C43D881120");
Debug.Assert(p2e4.Value() == 9);
Console.WriteLine(p2e4.AST());

var p2e5 = Packet.Parse("D8005AC2A8F0");
Debug.Assert(p2e5.Value() == 1);
Console.WriteLine(p2e5.AST());

var p2e6 = Packet.Parse("F600BC2D8F");
Debug.Assert(p2e6.Value() == 0);
Console.WriteLine(p2e6.AST());

var p2e7 = Packet.Parse("9C005AC2F8F0");
Debug.Assert(p2e7.Value() == 0);
Console.WriteLine(p2e7.AST());

var p2e8 = Packet.Parse("9C0141080250320F1802104A08");
Debug.Assert(p2e8.Value() == 1);
Console.WriteLine(p2e8.AST());

RunPart2();

void RunPart1()
{
    var hex = GetLines("myPuzzleInput.txt").First();
    var outerPacket = Packet.Parse(hex);
    Console.WriteLine($"Part one answer: {outerPacket.SumOfVersionsWithChildren()}");
}

void RunPart2()
{
    var hex = GetLines("myPuzzleInput.txt").First();
    var outerPacket = Packet.Parse(hex);

    var result = outerPacket.Value();

    var firstGuess = 1190591180u; // this was too low
    var secondGuess = 69910067916ul; // still too low!
    //                69910067916

    Debug.Assert(result > secondGuess);
    Console.WriteLine(outerPacket.AST());

    Console.WriteLine($"Part two answer: {result}");
}


public class BinaryNumber
{
    // todo: add growing beyond 64.
    public BinaryNumber()
    {
        Data = new(64);
        BitLength = 0;
    }
    public BitArray Data { get; private set; }
    public int BitLength { get; private set; }
    // Will set the four least significant bits to the four least significant bits of the passed byte.
    // If any data has already been set, the existing data will be pushed to be more significant by four
    // bits.
    public void PushNibble(byte nibble)
    {
        if (BitLength >= 64)
        {
            throw new OverflowException("Can't store more than 64 bits.");
        }
        if (BitLength > 0)
        {
            Data.ShiftLeft(4);
        }
        var setIndex = 64 - 1;
        Data.Set(setIndex--, (nibble & 1) == 1);
        Data.Set(setIndex--, (nibble & 2) == 2);
        Data.Set(setIndex--, (nibble & 4) == 4);
        Data.Set(setIndex--, (nibble & 8) == 8);
        BitLength += 4;
    }
    public ulong GetBigEndianULong() => Data.GetBigEndianULong();
    public override string ToString() => $"UInt64 Value (BE): {Data.GetBigEndianULong()}, Bits: {Data.Format()}";
    public string Formatted => ToString();
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

    public string AST()
    {
        switch ((PacketTypes)PacketType)
        {
            case PacketTypes.Sum:
                return $"({string.Join(" + ", SubPackets.Select(p => p.AST()))})";
            case PacketTypes.Product:
                return $"({string.Join(" * ", SubPackets.Select(p => p.AST()))})";
            case PacketTypes.Minimum:
                return $"Min({string.Join(", ", SubPackets.Select(p => p.AST()))})";
            case PacketTypes.Maximum:
                return $"Max({string.Join(", ", SubPackets.Select(p => p.AST()))})";
            case PacketTypes.Literal:
                return Data.GetBigEndianULong().ToString();
            case PacketTypes.GreaterThan:
                if (SubPackets.Count != 2)
                {
                    throw new NotSupportedException($"{nameof(PacketTypes.GreaterThan)} packets must have exactly two subpackets.");
                }
                return $"GT({string.Join(", ", SubPackets.Select(p => p.AST()))})";
            case PacketTypes.LessThan:
                if (SubPackets.Count != 2)
                {
                    throw new NotSupportedException($"{nameof(PacketTypes.LessThan)} packets must have exactly two subpackets.");
                }
                return $"LT({string.Join(", ", SubPackets.Select(p => p.AST()))})";
            case PacketTypes.EqualTo:
                if (SubPackets.Count != 2)
                {
                    throw new NotSupportedException($"{nameof(PacketTypes.EqualTo)} packets must have exactly two subpackets.");
                }
                return $"EQ({string.Join(", ", SubPackets.Select(p => p.AST()))})";
            default:
                throw new NotSupportedException($"Unknown Packet Type {PacketType}");
        }
    }

    public ulong Value()
    {
        switch ((PacketTypes)PacketType)
        {
            case PacketTypes.Sum:
                {
                    var result = 0ul;
                    foreach (var p in SubPackets)
                    {
                        result = checked(result + p.Value());
                    }
                    return result;
                }
            case PacketTypes.Product:
                { 
                    var result = 1ul;
                    foreach (var p in SubPackets)
                    {
                        result = checked(result * p.Value());
                    }
                    return result;
                }
            case PacketTypes.Minimum:
                return SubPackets.Min(p => p.Value());
            case PacketTypes.Maximum:
                return SubPackets.Max(p => p.Value());
            case PacketTypes.Literal:
                return Data.GetBigEndianULong();
            case PacketTypes.GreaterThan:
                if (SubPackets.Count != 2)
                {
                    throw new NotSupportedException($"{nameof(PacketTypes.GreaterThan)} packets must have exactly two subpackets.");
                }
                return SubPackets[0].Value() > SubPackets[1].Value() ? 1u : 0;
            case PacketTypes.LessThan:
                if (SubPackets.Count != 2)
                {
                    throw new NotSupportedException($"{nameof(PacketTypes.LessThan)} packets must have exactly two subpackets.");
                }
                return SubPackets[0].Value() < SubPackets[1].Value() ? 1u : 0;
            case PacketTypes.EqualTo:
                if (SubPackets.Count != 2)
                {
                    throw new NotSupportedException($"{nameof(PacketTypes.EqualTo)} packets must have exactly two subpackets.");
                }
                return SubPackets[0].Value() == SubPackets[1].Value() ? 1u : 0;
            default:
                throw new NotSupportedException($"Unknown Packet Type {PacketType}");
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
    private const int VERSION_LENGTH = 3;
    private const int PACKET_TYPE_LENGTH = 3;
    private const int LENGTH_TYPE_ID_LENGTH = 1;
    private const int DATA_LENGTH = 5;
    private const int COUNT_OF_SUBPACKETS_BITS_SIZE = 11;
    private const int SUBPACKETS_BIT_LENGTH_BITS_SIZE = 15;

    public bool IsLiteralValue => PacketType == (byte)PacketTypes.Literal;
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
    public enum PacketTypes
    {
        Sum = 0,
        Product = 1,
        Minimum = 2,
        Maximum = 3,
        Literal = 4,
        GreaterThan = 5,
        LessThan = 6,
        EqualTo = 7
    }
}