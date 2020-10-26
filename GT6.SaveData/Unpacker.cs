using System;
using System.IO;
using System.Linq;
using GT.SaveData.Crypt;
using GT.SaveData.GT6;
using GT.Shared;
using GT.Shared.Polyphony;
using GT.Shared.Polyphony.DataStructure;

namespace GT.SaveData {
    public class Unpacker {
        private readonly string _savePath;
        private readonly Game _game;
        public Unpacker(string saveFolder, Game game) {
            _savePath = saveFolder;
            _game = game;
        }

        public void Decrypt(bool decryptBbb = true) {
            switch (_game) {
                case Game.GTHD:
                case Game.GTPSP:
                case Game.GT5P:
                case Game.GT5TTC:
                    var files = Directory.GetFiles(_savePath, "GAME*.DAT", SearchOption.TopDirectoryOnly);
                    foreach (var file in files) {
                        DecryptFile(file);
                    }
                    break;
                case Game.GT6:
                    if (File.Exists(Path.Combine(_savePath, "PARAM.PFD")))
                        new SonyCrypt(_game).Decrypt(_savePath);

                    var decryptedFirst = DecryptGT6Files("GT6", decryptBbb);
                    var decryptedSecond = DecryptGT6Files("GT6_1", decryptBbb);

                    if (!decryptedFirst && !decryptedSecond) {
                        throw new Exception("Invalid GT6 save folder.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_game));
            }
        }

        private bool DecryptGT6Files(string prefix, bool decryptBbb = true) {
            try {
                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_toc_work"), DecryptFile(Path.Combine(_savePath, $"{prefix}.0")));
                var saveWorkData = DecryptFile(Path.Combine(_savePath, $"{prefix}.1"));

                if (decryptBbb) {
                    var saveWork = new SaveWork(saveWorkData);

                    // Read bank_book_blob btree
                    var bbbData = saveWork.BankBookBlob;
                    // Check if BBB is encrypted
                    if (bbbData[7] != 0x0E) {
                        var decryptedBbb = DecryptData(bbbData);
                        saveWork.BankBookBlob = decryptedBbb;
                        saveWorkData = saveWork.Save();
                    }
                }
                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_save_work"), saveWorkData);
                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_db_work"), DecryptFile(Path.Combine(_savePath, $"{prefix}.2")));
                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_garage_work"), DecryptFile(Path.Combine(_savePath, $"{prefix}.3")));
                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_garage_pad_stockyard"), DecryptFile(Path.Combine(_savePath, $"{prefix}.4")));

                var buffer = DecryptFile(Path.Combine(_savePath, $"{prefix}.5"));
                if (File.Exists(Path.Combine(_savePath, $"{prefix}.6"))) {
                    buffer = buffer.Concat(DecryptFile(Path.Combine(_savePath, $"{prefix}.6"))).ToArray();
                }

                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_garage_stockyard"), buffer);

                return true;
            }
            catch (Exception e) {
                return false;
            }
        }

        private static byte[] DecryptFile(string filePath) {
            var data = File.ReadAllBytes(filePath);
            return DecryptData(data);
        }

        private static byte[] DecryptData(byte[] data) {
            data = SwapBytes.ByteSwap(data);
#if DEBUG
            //File.WriteAllBytes($"{filePath}.byteswapped.bin", data);
#endif
            using (var ms = new MemoryStream(data))
            using (var ms2 = new MemoryStream())
            using (var reader = new EndianBinReader(ms))
            using (var writer = new EndianBinWriter(ms2)) {
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
}
