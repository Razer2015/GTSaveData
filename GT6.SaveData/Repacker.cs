using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GT.SaveData.Crypt;
using GT.SaveData.GT6;
using GT.Shared;
using GT.Shared.Polyphony;
using GT.SaveData.Hash;
using GT.Shared.Polyphony.DataStructure;

namespace GT.SaveData {
    public class Repacker {
        private static Random _rand = new Random();
        private readonly string _savePath;
        private readonly Game _game;

        public Repacker(string saveFolder, Game game) {
            _savePath = saveFolder;
            _game = game;
        }

        public void Encrypt(bool sonyLayer = true) {
            switch (_game) {
                case Game.GTHD:
                case Game.GTPSP:
                case Game.GT5P:
                case Game.GT5TTC:
                    var files = Directory.GetFiles(_savePath, "GAME*.DAT", SearchOption.TopDirectoryOnly);
                    foreach (var file in files) {
                        File.WriteAllBytes(file, EncryptFile(file));
                    }
                    break;
                case Game.GT6GC:
                case Game.GT6:
                    var encryptedFirst = EncryptGT6Files("GT6");
                    var encryptedSecond = EncryptGT6Files("GT6_1");

                    if (sonyLayer && File.Exists(Path.Combine(_savePath, "PARAM.PFD")))
                        new SonyCrypt(_game).Encrypt(_savePath);

                    if (!encryptedFirst && !encryptedSecond) {
                        throw new Exception("Invalid GT6 save folder.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_game));
            }
        }

        private bool EncryptGT6Files(string prefix) {
            try {
                var indexData = new GT6Index(File.ReadAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_toc_work")), _game);

                EncryptAndSetMetaInfo(indexData, Path.Combine(_savePath, $"{prefix}.tmp_save_work"));
                EncryptAndSetMetaInfo(indexData, Path.Combine(_savePath, $"{prefix}.tmp_db_work"));
                EncryptAndSetMetaInfo(indexData, Path.Combine(_savePath, $"{prefix}.tmp_garage_work"));
                if (File.Exists(Path.Combine(_savePath, $"{prefix}.tmp_garage_pad_stockyard"))) {
                    EncryptAndSetMetaInfo(indexData, Path.Combine(_savePath, $"{prefix}.tmp_garage_pad_stockyard"));
                }
                if (File.Exists(Path.Combine(_savePath, $"{prefix}.tmp_garage_stockyard"))) {
                    EncryptAndSetMetaInfo(indexData, Path.Combine(_savePath, $"{prefix}.tmp_garage_stockyard"));
                }
                
                // Encrypt the index file
                var buffer = EncryptData(indexData.GetBytes);
                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.0"), buffer);
                File.Delete(Path.Combine(_savePath, $"{prefix}.tmp_toc_work"));

                return true;
            }
            catch (Exception e) {
                return false;
            }
        }

        private void EncryptAndSetMetaInfo(GT6Index indexData, string fileName) {
            var buffer = File.ReadAllBytes(fileName);
            var fileLength = buffer.Length;
            MetaStruct metaInfo;
            switch (Path.GetExtension(fileName).TrimStart('.')) {
                case "tmp_toc_work":
                    buffer = EncryptData(buffer);
                    File.WriteAllBytes(Path.ChangeExtension(fileName, ".0"), buffer);
                    break;
                case "tmp_save_work":
                    metaInfo = indexData.GetMetaData(1);
                    metaInfo.FileLength = fileLength;
                    indexData.SetMetaData(1, metaInfo);

                    // Encrypt bank book blob first if necessary
                    if (_game == Game.GT6) {
                        var saveWork = new SaveWork(buffer);

                        // Read bank_book_blob btree
                        var bbbData = saveWork.BankBookBlob;
                        // Check if BBB is decrypted
                        if (bbbData[0] == 0x0E) {
                            var encryptedBbb = EncryptData(bbbData, false);
                            saveWork.BankBookBlob = encryptedBbb;
                            buffer = saveWork.Save();
                        }
                    }

                    buffer = EncryptData(buffer);
                    indexData.SetHash(1, ComputeTigerHash(buffer));
                    File.WriteAllBytes(Path.ChangeExtension(fileName, ".1"), buffer);
                    break;
                case "tmp_db_work":
                    metaInfo = indexData.GetMetaData(2);
                    metaInfo.FileLength = fileLength;
                    indexData.SetMetaData(2, metaInfo);
                    buffer = EncryptData(buffer);
                    indexData.SetHash(2, ComputeTigerHash(buffer));
                    File.WriteAllBytes(Path.ChangeExtension(fileName, ".2"), buffer);
                    break;
                case "tmp_garage_work":
                    metaInfo = indexData.GetMetaData(3);
                    metaInfo.FileLength = fileLength;
                    indexData.SetMetaData(3, metaInfo);
                    buffer = EncryptData(buffer);
                    indexData.SetHash(3, ComputeTigerHash(buffer));
                    File.WriteAllBytes(Path.ChangeExtension(fileName, ".3"), buffer);
                    break;
                case "tmp_garage_pad_stockyard":
                    metaInfo = indexData.GetMetaData(4);
                    metaInfo.FileLength = fileLength;
                    indexData.SetMetaData(4, metaInfo);
                    buffer = EncryptData(buffer);
                    indexData.SetHash(4, ComputeTigerHash(buffer));
                    File.WriteAllBytes(Path.ChangeExtension(fileName, ".4"), buffer);
                    break;
                case "tmp_garage_stockyard":
                    metaInfo = indexData.GetMetaData(5);
                    metaInfo.FileLength = fileLength;
                    indexData.SetMetaData(5, metaInfo);

                    var index5 = buffer.Take(0xFFFFF8).ToArray();
                    var index6 = buffer.Skip(0xFFFFF8).ToArray();
                    index5 = EncryptData(index5);
                    indexData.SetHash(5, ComputeTigerHash(index5));
                    File.WriteAllBytes(Path.ChangeExtension(fileName, ".5"), index5);

                    if (index6.Length > 0) {
                        index6 = EncryptData(index6);
                        indexData.SetHash(6, ComputeTigerHash(index6));
                        File.WriteAllBytes(Path.ChangeExtension(fileName, ".6"), index6);
                    }
                    break;
            }

            File.Delete(fileName);
        }

        private byte[] EncryptFile(string filePath, bool deflate = true) {
            return EncryptData(File.ReadAllBytes(filePath), deflate);
        }

        private byte[] EncryptData(byte[] data, bool deflate = true) {
            switch (_game) {
                case Game.GTHD:
                case Game.GTPSP:
                case Game.GT5P:
                    data = PS2Zip.Deflate(data);
                    break;
                case Game.GT5TTC:
                    data = PS2Zip.Deflate(data);
                    data = PS2Zip.Deflate(data);
                    break;
                case Game.GT6GC:
                case Game.GT6:
                    if (deflate)
                        data = PS2Zip.Deflate(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_game));
            }

            using (var ms = new MemoryStream(data))
            using (var ms2 = new MemoryStream())
            using (var reader = new EndianBinReader(ms))
            using (var writer = new EndianBinWriter(ms2)) {
                var bytes1 = _rand.Next();
                var bytes2 = (int)Crc32Checksum(data);

                writer.Write(bytes1);
                writer.Write(bytes2);

                var mt19937 = new MT19937();
                mt19937.init_genrand((ulong)(bytes1 + bytes2));

                var cipher = bytes1 ^ bytes2;
                while (reader.BaseStream.Position != reader.BaseStream.Length && reader.BaseStream.Position + 0x04 <= reader.BaseStream.Length) {
                    var output = StreamCipher.Cipher(ref cipher);
                    var read = reader.ReadInt32();
                    var combined = read ^ (int)mt19937.genrand_int32();

                    var result = combined - output;
                    writer.Write(result);
                }

                // Cipher the remainder if unequal to 4 bytes
                while (reader.BaseStream.Position != reader.BaseStream.Length) {
                    var output = StreamCipher.Cipher(ref cipher);
                    var read = reader.ReadByte();
                    var combined = read ^ (int)mt19937.genrand_int32();

                    var result = combined - output;
                    result &= 0xff;
                    writer.Write((byte)result);
                }

                var buffer = ms2.ToArray();
                buffer = SwapBytes.ByteSwap(buffer, _game);

                return buffer;
            }
        }

        private static byte[] ComputeTigerHash(byte[] data) {
            var tiger = new Tiger();
            tiger.ComputeHash(data);
            return tiger.Hash;
        }

        private static uint Crc32Checksum(byte[] data) {
            var result = ~Hash.Hash.CRC32_0x77073096(data);
            return result ^ 0x3039;
        }
    }
}
