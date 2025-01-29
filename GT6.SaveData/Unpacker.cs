using System;
using System.IO;
using GT.SaveData.Crypt;
using GT.SaveData.GT6;
using GT.Shared;
using GT.Shared.Polyphony;

namespace GT.SaveData {
    public class Unpacker {
        private readonly string _savePath;
        private readonly GameConfig _gameConfig;

        public Unpacker(string saveFolder) {
            _savePath = saveFolder;
            _gameConfig = new GameConfig(_savePath);
        }

        public Unpacker(string saveFolder, Game game)
        {
            _savePath = saveFolder;
            _gameConfig = new GameConfig(game);
        }

        public void Decrypt(bool decryptBbb = true) {
            switch (_gameConfig.DetermineGame()) {
                case Game.GTHD:
                case Game.GTPSP:
                case Game.GT5P:
                case Game.GT5TTC:
                    var files = Directory.GetFiles(_savePath, "GAME*.DAT", SearchOption.TopDirectoryOnly);
                    foreach (var file in files) {
                        File.WriteAllBytes(file, DecryptFile(file));
                    }
                    break;
                case Game.GT6GC:
                case Game.GT6:
                    // if (File.Exists(Path.Combine(_savePath, "PARAM.PFD")))
                    //     new SonyCrypt(_gameConfig.DetermineGame()).Decrypt(_savePath);

                    var decryptedFirst = DecryptGt6Files("GT6", decryptBbb);
                    var decryptedSecond = DecryptGt6Files("GT6_1", decryptBbb);

                    if (!decryptedFirst && !decryptedSecond) {
                        throw new Exception("Invalid GT6 save folder.");
                    }
                    break;
                case Game.GT5:
                    if (File.Exists(Path.Combine(_savePath, "PARAM.PFD")))
                        new SonyCrypt(_gameConfig.DetermineGame()).Decrypt(_savePath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_gameConfig));
            }
        }

        private bool DecryptGt6Files(string prefix, bool decryptBbb = true) {
            try {
                if (!File.Exists(Path.Combine(_savePath, $"{prefix}.0"))) return false;

                // Decrypt the TOC file and parse it
                var tocBuffer = DecryptFile(Path.Combine(_savePath, $"{prefix}.0"));
                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_toc_work"), tocBuffer);

                var toc = new Gt6Index(tocBuffer, _gameConfig.DetermineGame());
                var files = toc.GetMetaDatas();
                if (files.Length == 0) return false;

                foreach (var file in files)
                {
                    var processingFile = Path.GetFileNameWithoutExtension(file.FilePath);

                    byte[] buffer;
                    using (var memoryStream = new MemoryStream())
                    {
                        foreach (var fileIndex in file.FileIndexes)
                        {
// #if DEBUG
//                             File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.{fileIndex}_dec"), DecryptFile(Path.Combine(_savePath, $"{prefix}.{fileIndex}")));
// #endif
                            var fileBuffer = DecryptFile(Path.Combine(_savePath, $"{prefix}.{fileIndex}"));
                            memoryStream.Write(fileBuffer, 0, fileBuffer.Length);
                        }
                        buffer = memoryStream.ToArray();
                    }

                    if (processingFile.Equals("tmp_save_work") && _gameConfig.DetermineGame() == Game.GT6 && decryptBbb) {
                        var saveWork = new SaveWork(buffer);

                        // Read bank_book_blob btree
                        var bbbData = saveWork.BankBookBlob;
                        // Check if BBB is encrypted
                        if (bbbData[7] != 0x0E) {
                            var decryptedBbb = DecryptData(bbbData, _gameConfig.DetermineGame());
                            saveWork.BankBookBlob = decryptedBbb;
                            buffer = saveWork.Save();
                        }
                    }

                    File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.{processingFile}"), buffer);
                }

                return true;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private byte[] DecryptFile(string filePath) {
            var data = File.Exists(Path.Combine(_savePath, "PARAM.PFD")) 
                ? new SonyCrypt(_gameConfig.DetermineGame()).DecryptFileToBytes(filePath) 
                : File.ReadAllBytes(filePath);
            // var data = File.ReadAllBytes(filePath);

            if (data == null)
            {
                throw new Exception($"Failed to decrypt file {filePath}.");
            }
            
            return DecryptData(data, _gameConfig.DetermineGame());
        }

        public static byte[] DecryptData(byte[] data, Game game) {
            data = SwapBytes.ByteSwap(data, game);
#if DEBUG
            //File.WriteAllBytes($"{filePath}.byteswapped.bin", data);
#endif
            using var ms = new MemoryStream(data);
            using var ms2 = new MemoryStream();
            using var reader = new EndianBinReader(ms);
            using var writer = new EndianBinWriter(ms2);
            var bytes1 = reader.ReadInt32();
            var bytes2 = reader.ReadInt32();

            var mt19937 = new MT19937();
            mt19937.init_genrand((ulong)(bytes1 + bytes2));

            var cipher = bytes1 ^ bytes2;
            while (reader.BaseStream.Position != reader.BaseStream.Length && reader.BaseStream.Position + 0x04 <= reader.BaseStream.Length) {
                var output = StreamCipher.Cipher(ref cipher);
                var read = reader.ReadInt32();
                var combined = read + output;

                var result = combined ^ (int)mt19937.genrand_int32();
                writer.Write(result);
            }

            // Cipher the remainder if unequal to 4 bytes
            while (reader.BaseStream.Position != reader.BaseStream.Length) {
                var output = StreamCipher.Cipher(ref cipher);
                var read = reader.ReadByte();
                var combined = read + output;

                var result = combined ^ (int)mt19937.genrand_int32();
                result &= 0xff;
                writer.Write((byte)result);
            }

            var buffer = ms2.ToArray();
#if DEBUG
            //File.WriteAllBytes($"{filePath}.decrypted.bin", buffer);
#endif
            if (Util.DataAtUInt32(buffer, 0) == 0xC5EEF7FFu)
                buffer = PS2Zip.Inflate(buffer);

            if (Util.DataAtUInt32(buffer, 0) == 0xC5EEF7FFu) // Don't look at me, ask PD why some saves are double compressed
                buffer = PS2Zip.Inflate(buffer);

#if DEBUG
            //File.WriteAllBytes($"{filePath}.decompressed.bin", buffer);
#endif
            return buffer;
        }
    }
}
