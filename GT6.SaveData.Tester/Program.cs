using GT.SaveData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GT.SaveData.Hash;
using System.Diagnostics;
using System.Reflection;

namespace GT.SaveData.Tester {
    class Program {
        static void PrintInfo() {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            Console.WriteLine($"Version {version}");
            Console.WriteLine("Coded by xfileFIN (Team eventHorizon)");
            Console.WriteLine(@"Credits: 
    - Echelo and q-k for reverse engineering the encryption and hashes used.
    - Nenkai for solving the correct GT6 tmp_save_work header.");
            Console.WriteLine();
            Console.WriteLine("Usage: <operation> <saveFolder>");
            Console.WriteLine("Operations:");
            Console.WriteLine("    d or --decrypt");
            Console.WriteLine("    e or --encrypt");
        }

        static void Main(string[] args) {
            if (args.Length != 2) {
                PrintInfo();

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            if (args[0].Equals("d", StringComparison.OrdinalIgnoreCase) ||
                args[0].Equals("--decrypt", StringComparison.OrdinalIgnoreCase)) {
                new Unpacker(args[1], Game.GT6).Decrypt();
                Console.WriteLine("Save successfully decrypted.");
            }
            else if (args[0].Equals("e", StringComparison.OrdinalIgnoreCase) ||
                     args[0].Equals("--encrypt", StringComparison.OrdinalIgnoreCase)) {
                try {
                    new Repacker(args[1], Game.GT6).Encrypt();
                    Console.WriteLine("Save successfully encrypted.");
                }
                catch (Exception ex) {
                    Console.WriteLine("Unable to encrypt. Are you sure the save is decrypted?");
                }
            }
            else {
                PrintInfo();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

#if DEBUG
            ////Console.WriteLine(Crc32.Compute(File.ReadAllBytes("without_crc.bin")).ToString("X8"));
            ////Console.WriteLine(Checksum(File.ReadAllBytes("without_crc.bin")).ToString("X8"));

            ////var files = Directory.GetFiles(@"HashCheck", "*.?", SearchOption.TopDirectoryOnly);
            ////foreach (var file in files) {
            ////    Console.WriteLine(ByteArrayToString(ComputeTigerHash(File.ReadAllBytes(file))));
            ////}

            ////var myhash = new Tiger();
            ////myhash.ComputeHash(File.ReadAllBytes(@"GT6Save\GT6_1.1"));
            ////byte[] the_hash_result = myhash.Hash;

            ////var filePath = @"C:\Users\xfile\RPCS3\dev_hdd0\home\00000001\savedata\BCJS37016-GAME6";
            //var filePath = @"SonyLayerTest";
            ////var filePath = "BCJS37016-GAME6";
            ////var filePath = "GT6Save";
            ////var filePath = "HashCheck";
            //new Unpacker(filePath, Game.GT6).Decrypt();
            //new Repacker(filePath, Game.GT6).Encrypt();
            ////new Unpacker(filePath, Game.GT6).Decrypt();
#endif
        }

        private static byte[] ComputeTigerHash(byte[] data) {
            var tiger = new Tiger();
            tiger.ComputeHash(data);
            return tiger.Hash;
        }

        private static string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }

        static uint Checksum(byte[] data) {
            uint result = ~Hash.Hash.CRC32_0x77073096(data);
            return result;
        }
    }
}
