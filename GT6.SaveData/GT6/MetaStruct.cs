using System.Text;

namespace GT.SaveData.GT6 {
    public struct MetaStruct {
        public string FilePath { get; set; }
        public int FileLength { get; set; }
        public byte Index { get; set; }

        public MetaStruct(EndianBinReader reader) {
            FilePath = Encoding.UTF8.GetString(reader.ReadBytes(0x100));
            FileLength = reader.ReadInt32();
            Index = reader.ReadByte();
        }

        public void Write(EndianBinWriter writer) {
            writer.BaseStream.Position += 0x100; // Skipping Cache filepath for now
            writer.Write(FileLength);
            writer.Write(Index);
        }
    }
}
