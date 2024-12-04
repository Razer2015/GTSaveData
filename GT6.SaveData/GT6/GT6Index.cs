using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GT.SaveData.GT6
{
    public class Gt6Index
    {

        private const int BlockCount = 0x0A;

        private const int HashLength = 0x18;

        private readonly int _blockLength = 0x168;

        private readonly int _hashTableOffset = 0x0E28;

        public Gt6Index(byte[] data, Game game = Game.GT6)
        {
            _blockLength = game.Equals(Game.GT6GC) ? 0x1CC : 0x168;
            _hashTableOffset = game.Equals(Game.GT6GC) ? 0x1210 : 0x0E28;
            GetBytes = data;
        }

        public Gt6Index(string fileName)
        {
            if (!File.Exists(fileName)) throw new FileNotFoundException(fileName);

            GetBytes = File.ReadAllBytes(fileName);
        }

        /// <summary>
        ///     Get all the possible meta information
        /// </summary>
        /// <returns></returns>
        public MetaStruct[] GetMetaDatas()
        {
            using var ms = new MemoryStream(GetBytes);
            using var reader = new EndianBinReader(ms);
            var metaStructs = new List<MetaStruct>();
            
            var index = 0;
            while (reader.PeekChar() != (char)0x00)
            {
                metaStructs.Add(new MetaStruct(reader));
                reader.BaseStream.Position = ++index * _blockLength;
            }
            
            return metaStructs.ToArray();
        }

        /// <summary>
        ///     Get the meta information for the give index
        /// </summary>
        /// <param name="index">GT6.X or GT6_1.X - the X part of the filename</param>
        /// <returns></returns>
        public MetaStruct GetMetaData(byte index)
        {
            using var ms = new MemoryStream(GetBytes);
            using var reader = new EndianBinReader(ms);
            reader.BaseStream.Position = (index - 1) * _blockLength;
            return new MetaStruct(reader);
        }
        
        /// <summary>
        ///     Get the next file index
        /// </summary>
        public byte GetNextFileIndex()
        {
            var metaStructs = GetMetaDatas();
            var usedIndexes = metaStructs.SelectMany(x => x.FileIndexes).ToHashSet();
            byte index = 1;

            while (usedIndexes.Contains(index))
            {
                index++;
            }

            return index;
        }

        /// <summary>
        ///     Get the hash of the file
        /// </summary>
        /// <param name="index">GT6.X or GT6_1.X - the X part of the filename</param>
        /// <returns></returns>
        public byte[] GetHash(byte index)
        {
            using var ms = new MemoryStream(GetBytes);
            using var reader = new EndianBinReader(ms);
            reader.BaseStream.Position = _hashTableOffset + index * HashLength;
            return reader.ReadBytes(HashLength);
        }

        /// <summary>
        ///     Save the new meta information
        /// </summary>
        /// <param name="index">GT6.X or GT6_1.X - the X part of the filename</param>
        /// <param name="metaInfo">MetaData to be saved</param>
        public void SetMetaData(byte index, MetaStruct metaInfo)
        {
            using var ms = new MemoryStream(GetBytes);
            using var writer = new EndianBinWriter(ms);
            writer.BaseStream.Position = (index - 1) * _blockLength;
            metaInfo.Write(writer);
            GetBytes = ms.ToArray();
        }

        /// <summary>
        ///     Save a new hash
        /// </summary>
        /// <param name="index">GT6.X or GT6_1.X - the X part of the filename</param>
        /// <param name="hash">New hash to be saved</param>
        /// <returns></returns>
        public void SetHash(byte index, byte[] hash)
        {
            using var ms = new MemoryStream(GetBytes);
            using var writer = new EndianBinWriter(ms);
            writer.BaseStream.Position = _hashTableOffset + (index - 1) * HashLength;
            writer.Write(hash);
            GetBytes = ms.ToArray();
        }

        public byte[] GetBytes { get; private set; }

    }
}