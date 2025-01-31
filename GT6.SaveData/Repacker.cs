﻿using System;
using System.IO;
using GT.SaveData.Crypt;
using GT.SaveData.GT6;
using GT.Shared.Polyphony;
using GT.SaveData.Hash;

namespace GT.SaveData {
    public class Repacker {
        private static readonly Random Rand = new Random();
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
                    string[] files = Directory.GetFiles(_savePath, "GAME*.DAT", SearchOption.TopDirectoryOnly);
                    foreach (string file in files) {
                        File.WriteAllBytes(file, EncryptFile(file));
                    }
                    break;
                case Game.GT6GC:
                case Game.GT6:
                    bool encryptedFirst = EncryptGt6Files("GT6");
                    bool encryptedSecond = EncryptGt6Files("GT6_1");

                    if (sonyLayer && File.Exists(Path.Combine(_savePath, "PARAM.PFD")))
                        new SonyCrypt(_game).Encrypt(_savePath);

                    if (!encryptedFirst && !encryptedSecond) {
                        throw new Exception("Invalid GT6 save folder.");
                    }
                    break;
                case Game.GT5:
                    if (File.Exists(Path.Combine(_savePath, "PARAM.PFD")))
                        new SonyCrypt(_game).Encrypt(_savePath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_game));
            }
        }

        private bool EncryptGt6Files(string prefix) {
            try {
                var toc = GetIndexFile(prefix);
                if (toc == null) return false;

                // Encrypt each file and set the meta info
                byte index = 1;
                foreach (var fileMeta in toc.GetMetaDatas()) {
                    string filePath = Path.Combine(_savePath, $"{prefix}.{Path.GetFileNameWithoutExtension(fileMeta.FilePath)}");
                    if (File.Exists(filePath)) {
                        EncryptAndSetMetaInfo(toc, index, filePath);
                    }
                    index++;
                }

                // Encrypt the index file
                byte[] buffer = EncryptData(toc.GetBytes);
                File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.0"), buffer);
                File.Delete(Path.Combine(_savePath, $"{prefix}.tmp_toc_work"));

                return true;
            } catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
        }

        private void EncryptAndSetMetaInfo(Gt6Index indexData, byte index, string fileName) {
            byte[] buffer = File.ReadAllBytes(fileName);

            // Encrypt bank book blob first if necessary
            if (Path.GetExtension(fileName).Equals(".tmp_save_work") && _game == Game.GT6) {
                var saveWork = new SaveWork(buffer);

                // Read bank_book_blob btree
                byte[] bbbData = saveWork.BankBookBlob;
                // Check if BBB is decrypted
                if (bbbData[0] == 0x0E) {
                    byte[] encryptedBbb = EncryptData(bbbData, false);
                    saveWork.BankBookBlob = encryptedBbb;
                    buffer = saveWork.Save();
                }
            }

            int fileLength = buffer.Length;
            var metaInfo = indexData.GetMetaData(index);

            const int maxFileSize = 0xFFFFF8;
            int requiredParts = (fileLength + maxFileSize - 1) / maxFileSize;
            int currentParts = metaInfo.FileIndexes.Count;


            // If there were more currentParts than requiredParts, remove the file indexes
            for (int i = requiredParts; i < currentParts; i++)
            {
                metaInfo.FileIndexes.RemoveAt(i);
                indexData.SetMetaData(index, metaInfo);
            }

            // If there is only one part, encrypt and set the hash
            // The loop below can handle this case but there's unnecessary Array.Copy so we handle this separately
            if (requiredParts == 1)
            {
                byte partIndex = metaInfo.FileIndexes[0];
                buffer = EncryptData(buffer);
                indexData.SetHash(partIndex, ComputeTigerHash(buffer));
                File.WriteAllBytes(Path.ChangeExtension(fileName, $".{partIndex}"), buffer);

                File.Delete(fileName);
                return;
            }

            // Encrypt and set the hash for each part
            for (int i = 0; i < requiredParts; i++)
            {
                if (i >= currentParts)
                {
                    byte nextAvailableFileIndex = indexData.GetNextFileIndex();
                    metaInfo.FileIndexes.Add(nextAvailableFileIndex);
                    indexData.SetMetaData(index, metaInfo);
                }

                byte partIndex = metaInfo.FileIndexes[i];
                int chunkSize = Math.Min(maxFileSize, buffer.Length - i * maxFileSize);
                byte[] chunk = new byte[chunkSize];
                Array.Copy(buffer, i * maxFileSize, chunk, 0, chunkSize);
                chunk = EncryptData(chunk);
                indexData.SetHash(partIndex, ComputeTigerHash(chunk));
                File.WriteAllBytes(Path.ChangeExtension(fileName, $".{partIndex}"), chunk);
            }

            File.Delete(fileName);
        }

        private Gt6Index? GetIndexFile(string prefix)
        {
            string indexFile = Path.Combine(_savePath, "GT6.tmp_toc_work");
            if (File.Exists(indexFile)) {
                return new Gt6Index(File.ReadAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_toc_work")), _game);
            }

            if (!File.Exists(Path.Combine(_savePath, $"{prefix}.0")))
            {
                return null;
            }
            
            var filePath = Path.Combine(_savePath, $"{prefix}.0");
            var tocBuffer = File.Exists(Path.Combine(_savePath, "PARAM.PFD")) 
                ? new SonyCrypt(_game).DecryptFileToBytes(filePath) 
                : File.ReadAllBytes(filePath);
            tocBuffer = Unpacker.DecryptData(tocBuffer, _game);
            File.WriteAllBytes(Path.Combine(_savePath, $"{prefix}.tmp_toc_work"), tocBuffer);

            return new Gt6Index(tocBuffer, _game);
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
                case Game.GT5:
                default:
                    throw new ArgumentOutOfRangeException(nameof(_game));
            }

            using var ms = new MemoryStream(data);
            using var ms2 = new MemoryStream();
            using var reader = new EndianBinReader(ms);
            using var writer = new EndianBinWriter(ms2);
            int bytes1 = Rand.Next();
            int bytes2 = (int)Crc32Checksum(data);

            writer.Write(bytes1);
            writer.Write(bytes2);

            var mt19937 = new MT19937();
            mt19937.init_genrand((ulong)(bytes1 + bytes2));

            int cipher = bytes1 ^ bytes2;
            while (reader.BaseStream.Position != reader.BaseStream.Length && reader.BaseStream.Position + 0x04 <= reader.BaseStream.Length) {
                int output = StreamCipher.Cipher(ref cipher);
                int read = reader.ReadInt32();
                int combined = read ^ (int)mt19937.genrand_int32();

                int result = combined - output;
                writer.Write(result);
            }

            // Cipher the remainder if unequal to 4 bytes
            while (reader.BaseStream.Position != reader.BaseStream.Length) {
                int output = StreamCipher.Cipher(ref cipher);
                byte read = reader.ReadByte();
                int combined = read ^ (int)mt19937.genrand_int32();

                int result = combined - output;
                result &= 0xff;
                writer.Write((byte)result);
            }

            byte[] buffer = ms2.ToArray();
            buffer = SwapBytes.ByteSwap(buffer, _game);

            return buffer;
        }

        private static byte[] ComputeTigerHash(byte[] data) {
            var tiger = new Tiger();
            tiger.ComputeHash(data);
            return tiger.Hash;
        }

        private static uint Crc32Checksum(byte[] data) {
            uint result = ~Hash.Hash.CRC32_0x77073096(data);
            return result ^ 0x3039;
        }
    }
}
