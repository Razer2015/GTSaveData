using System.Collections.Generic;
using System.Text;

namespace GT.SaveData.GT6
{
    public struct MetaStruct
    {

        public string FilePath { get; set; }

        public int FileLength { get; set; }

        public List<byte> FileIndexes { get; set; }

        public MetaStruct(EndianBinReader reader)
        {
            FilePath = Encoding.UTF8.GetString(reader.ReadBytes(0x100));
            FileLength = reader.ReadInt32();
            
            FileIndexes = new List<byte>();
            int index;
            while ((index = reader.ReadByte()) > 0)
            {
                FileIndexes.Add((byte)index);
            }
        }

        public void Write(EndianBinWriter writer)
        {
            writer.BaseStream.Position += 0x100; // Skipping Cache filepath for now
            writer.Write(FileLength);

            // In GT6 the file index table can hold 100 indexes
            // In GT6 GameScom demo the file index table can hold 200 indexes
            // Theoretically the maximum file size could have 256 indexes but the table doesn't support that
            
            // Going to write 100 indexes for now, very unlikely to have more than 2
            for (byte i = 0; i < 100; i++)
            {
                if (i < FileIndexes.Count)
                {
                    writer.Write(FileIndexes[i]);
                }
                else
                {
                    writer.Write((byte)0);
                }
            }
        }

    }
}