using System;
using System.IO;
using GT.Shared.Polyphony.DataStructure;

namespace GT.SaveData.GT6 {
    public class SaveWork {
        private Header _header;
        private PDTree _pdTree;
        private byte[] _data;

        public SaveWork(byte[] data) {
            _data = data;
            Read();
        }

        public byte[] Save() {
            return Write();
        }

        public byte[] BankBookBlob {
            get {

                return (byte[])_pdTree.PDTreeData.Fields.GetDataBySymbol(new[] { "user_profile", "bank_book_blob" }); ;
            }
            set {
                _pdTree.PDTreeData.Fields.SetDataBySymbol(new[] { "user_profile", "bank_book_blob" }, value);
            }
        }

        private void Read() {
            using (var ms = new MemoryStream(_data))
            using (var reader = new EndianBinReader(ms)) {
                _header = new Header(reader);

                _pdTree = new PDTree(_data, Shared.Polyphony.DataStructure.Game.GT6);
                _pdTree.Read();
            }
        }

        private byte[] Write() {
            using (var ms = new MemoryStream())
            using (var writer = new EndianBinWriter(ms)) {
                _pdTree.Write();
                _header.Length = _pdTree.FileData.Length;
                _header.Write(writer);
                writer.Write(_pdTree.FileData);

                return ms.ToArray();
            }
        }

        internal struct Header {
            internal int Magic { get; set; }
            internal long Version { get; set; }
            internal int Length { get; set; }

            public Header(EndianBinReader reader) {
                if ((Magic = reader.ReadInt32()) != 0x00000018)
                    throw new Exception("Header for GT6 save_work file is invalid.");

                Version = reader.ReadInt64();
                Length = reader.ReadInt32();
            }

            public void Write(EndianBinWriter writer) {
                writer.Write(Magic);
                writer.Write(Version);
                writer.Write(Length);
            }
        }
    }
}
