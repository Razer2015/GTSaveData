using GT.Shared.Polyphony;
using System;
using System.IO;

public class EndianBinReader : BinaryReader {
    public EndianType Endianess { get; set; }

    public EndianBinReader(Stream stream, EndianType endian = EndianType.BIG_ENDIAN) : base(stream) {
        Endianess = endian;
    }

    public double ReadDouble(EndianType endianess) {
        var memorize = Endianess;
        Endianess = endianess;
        var value = ReadDouble();
        Endianess = memorize;
        return value;
    }

    public override double ReadDouble() {
        byte[] array = base.ReadBytes(8);
        if (Endianess == EndianType.BIG_ENDIAN)
            Array.Reverse(array);
        return BitConverter.ToDouble(array, 0);
    }

    public double ReadInt16(EndianType endianess) {
        var memorize = Endianess;
        Endianess = endianess;
        var value = ReadInt16();
        Endianess = memorize;
        return value;
    }

    public override short ReadInt16() {
        byte[] array = base.ReadBytes(2);
        if (Endianess == EndianType.BIG_ENDIAN)
            Array.Reverse(array);
        return BitConverter.ToInt16(array, 0);
    }

    public double ReadInt32(EndianType endianess) {
        var memorize = Endianess;
        Endianess = endianess;
        var value = ReadInt32();
        Endianess = memorize;
        return value;
    }

    public override int ReadInt32() {
        byte[] array = base.ReadBytes(4);
        if (Endianess == EndianType.BIG_ENDIAN)
            Array.Reverse(array);
        return BitConverter.ToInt32(array, 0);
    }

    public double ReadInt64(EndianType endianess) {
        var memorize = Endianess;
        Endianess = endianess;
        var value = ReadInt64();
        Endianess = memorize;
        return value;
    }

    public override long ReadInt64() {
        byte[] array = base.ReadBytes(8);
        if (Endianess == EndianType.BIG_ENDIAN)
            Array.Reverse(array);
        return BitConverter.ToInt64(array, 0);
    }

    public double ReadSingle(EndianType endianess) {
        var memorize = Endianess;
        Endianess = endianess;
        var value = ReadSingle();
        Endianess = memorize;
        return value;
    }

    public override float ReadSingle() {
        byte[] array = base.ReadBytes(4);
        if (Endianess == EndianType.BIG_ENDIAN)
            Array.Reverse(array);
        return BitConverter.ToSingle(array, 0);
    }

    public double ReadUInt16(EndianType endianess) {
        var memorize = Endianess;
        Endianess = endianess;
        var value = ReadUInt16();
        Endianess = memorize;
        return value;
    }

    public override ushort ReadUInt16() {
        byte[] array = base.ReadBytes(2);
        if (Endianess == EndianType.BIG_ENDIAN)
            Array.Reverse(array);
        return BitConverter.ToUInt16(array, 0);
    }

    public double ReadUInt32(EndianType endianess) {
        var memorize = Endianess;
        Endianess = endianess;
        var value = ReadUInt32();
        Endianess = memorize;
        return value;
    }

    public override uint ReadUInt32() {
        byte[] array = base.ReadBytes(4);
        if (Endianess == EndianType.BIG_ENDIAN)
            Array.Reverse(array);
        return BitConverter.ToUInt32(array, 0);
    }

    public double ReadUInt64(EndianType endianess) {
        var memorize = Endianess;
        Endianess = endianess;
        var value = ReadUInt64();
        Endianess = memorize;
        return value;
    }

    public override ulong ReadUInt64() {
        byte[] array = base.ReadBytes(8);
        if (Endianess == EndianType.BIG_ENDIAN)
            Array.Reverse(array);
        return BitConverter.ToUInt64(array, 0);
    }

    public int Read3BytesInt32() {
        byte[] buffer = base.ReadBytes(3);
        return ((buffer[2] << 0) | (buffer[1] << 8) | (buffer[0] << 16));
    }

    public uint Read3BytesUInt32() {
        byte[] buffer = base.ReadBytes(3);
        return (uint)((buffer[2] << 0) | (buffer[1] << 8) | (buffer[0] << 16));
    }

    public double ReadDoubleR() {
        byte[] array = base.ReadBytes(8);
        return BitConverter.ToDouble(array, 0);
    }

    public short ReadInt16R() {
        byte[] array = base.ReadBytes(2);
        return BitConverter.ToInt16(array, 0);
    }

    public int ReadInt32R() {
        byte[] array = base.ReadBytes(4);
        return BitConverter.ToInt32(array, 0);
    }

    public long ReadInt64R() {
        byte[] array = base.ReadBytes(8);
        return BitConverter.ToInt64(array, 0);
    }

    public float ReadSingleR() {
        byte[] array = base.ReadBytes(4);
        return BitConverter.ToSingle(array, 0);
    }

    public ushort ReadUInt16R() {
        byte[] array = base.ReadBytes(2);
        return BitConverter.ToUInt16(array, 0);
    }

    public uint ReadUInt32R() {
        byte[] array = base.ReadBytes(4);
        return BitConverter.ToUInt32(array, 0);
    }


    public ulong ReadUInt64R() {
        byte[] array = base.ReadBytes(8);
        return BitConverter.ToUInt64(array, 0);
    }
}

