using System.IO;

namespace GT.SaveData.GT6 {
    public class GT6Index {
        private const int BLOCKLENGTH = 0x168;
        private const int BLOCKCOUNT = 0x0A;
        private const int HASHTABLE_OFFSET = 0x0E28;
        private const int HASH_LENGTH = 0x18;

        private byte[] _data;

        public GT6Index(byte[] data) {
            _data = data;
        }

        public GT6Index(string fileName) {
            if (!File.Exists(fileName)) throw new FileNotFoundException(fileName);

            _data = File.ReadAllBytes(fileName);
        }

        /// <summary>
        ///     Get all the possible meta informations
        /// </summary>
        /// <returns></returns>
        public MetaStruct[] GetMetaDatas() {
            using (var ms = new MemoryStream(_data))
            using (var reader = new EndianBinReader(ms)) {
                var metaStructs = new MetaStruct[BLOCKCOUNT];
                for (var i = 0; i < BLOCKCOUNT; i++) {
                    reader.BaseStream.Position = i * BLOCKLENGTH;
                    metaStructs[i] = new MetaStruct(reader);
                }

                return metaStructs;
            }
        }

        /// <summary>
        ///     Get the meta information for the give index
        /// </summary>
        /// <param name="index">GT6.X or GT6_1.X - the X part of the filename</param>
        /// <returns></returns>
        public MetaStruct GetMetaData(byte index) {
            using (var ms = new MemoryStream(_data))
            using (var reader = new EndianBinReader(ms)) {
                reader.BaseStream.Position = (index - 1) * BLOCKLENGTH;
                return new MetaStruct(reader);
            }
        }

        /// <summary>
        ///     Get the hash of the file
        /// </summary>
        /// <param name="index">GT6.X or GT6_1.X - the X part of the filename</param>
        /// <returns></returns>
        public byte[] GetHash(byte index) {
            using (var ms = new MemoryStream(_data))
            using (var reader = new EndianBinReader(ms)) {
                reader.BaseStream.Position = HASHTABLE_OFFSET + index * HASH_LENGTH;
                return reader.ReadBytes(HASH_LENGTH);
            }
        }

        /// <summary>
        ///     Save the new meta information
        /// </summary>
        /// <param name="index">GT6.X or GT6_1.X - the X part of the filename</param>
        /// <param name="metaInfo">MetaData to be saved</param>
        public void SetMetaData(byte index, MetaStruct metaInfo) {
            using (var ms = new MemoryStream(_data))
            using (var writer = new EndianBinWriter(ms)) {
                writer.BaseStream.Position = (index - 1) * BLOCKLENGTH;
                metaInfo.Write(writer);
                _data = ms.ToArray();
            }
        }

        /// <summary>
        ///     Save a new hash
        /// </summary>
        /// <param name="index">GT6.X or GT6_1.X - the X part of the filename</param>
        /// <param name="hash">New hash to be saved</param>
        /// <returns></returns>
        public void SetHash(byte index, byte[] hash) {
            using (var ms = new MemoryStream(_data))
            using (var writer = new EndianBinWriter(ms)) {
                writer.BaseStream.Position = HASHTABLE_OFFSET + (index - 1) * HASH_LENGTH;
                writer.Write(hash);
                _data = ms.ToArray();
            }
        }

        public byte[] GetBytes => _data;
    }
}
