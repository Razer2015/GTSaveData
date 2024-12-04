using System.Diagnostics;
using System.Reflection;
using System.Text;
using GT.SaveData.Hash;

namespace GT.SaveData.Console;

class Program
{

    private static void PrintInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        var version = fvi.FileVersion;

        System.Console.WriteLine($"Version {version}");
        System.Console.WriteLine("Coded by xfileFIN (Team eventHorizon)");
        System.Console.WriteLine(@"Credits: 
    - Echelo and q-k for reverse engineering the encryption and hashes used.
    - Nenkai for solving the correct GT6 tmp_save_work header.");
        System.Console.WriteLine();
        System.Console.WriteLine("Usage: <operation> <saveFolder>");
        System.Console.WriteLine("Operations:");
        System.Console.WriteLine("    d or --decrypt");
        System.Console.WriteLine("    e or --encrypt");
    }

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            PrintInfo();

            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
            return;
        }

        if (args.Length == 1)
        {
            var attr = File.GetAttributes(args[0]);
            if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
            {
                PrintInfo();

                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
                return;
            }

            System.Console.Write("Select the operation decrypt/encrypt (d or e): ");
            var response = System.Console.ReadLine();
            switch (response)
            {
                case "d":
                    args = new string[]
                    {
                        "d",
                        args[0]
                    };
                    break;
                case "e":
                    args = new string[]
                    {
                        "e",
                        args[0]
                    };
                    break;
                default:
                    System.Console.WriteLine("Invalid option, expected d for decrypt or e for encrypt.");
                    System.Console.WriteLine();
                    System.Console.WriteLine("Press any key to exit...");
                    System.Console.ReadKey();
                    return;
            }
        }

        var gameCode = Path.GetFileName(args[1]);
        var game = GetGame(gameCode);

        if (args[0].Equals("d", StringComparison.OrdinalIgnoreCase) ||
            args[0].Equals("--decrypt", StringComparison.OrdinalIgnoreCase))
        {
            new Unpacker(args[1]).Decrypt();
            System.Console.WriteLine("Save successfully decrypted.");
        }
        else if (args[0].Equals("e", StringComparison.OrdinalIgnoreCase) ||
                 args[0].Equals("--encrypt", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                new Repacker(args[1], game).Encrypt();
                System.Console.WriteLine("Save successfully encrypted.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Unable to encrypt. Are you sure the save is decrypted?");
            }
        }
        else
        {
            PrintInfo();
        }

        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();

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

    private static Game GetGame(string gameCode)
    {
        return gameCode switch
        {
            "NPEA90002-GAME-" => Game.GTHD,
            "NPJA90061-GAME-" or "BCJS30017-GAME-" => Game.GT5P,
            "NPHA80080-GAME-" or "NPUA70087-GAME-" or "NPEA90052-GAME-" => Game.GT5TTC,
            "BCJS37016-GAME6" or "BCJS37016-BKUP6" => Game.GT6,
            "DEMO32768-GAME6" or "DEMO32768-BKUP6" => Game.GT6GC,
            _ => throw new NotImplementedException(
                "Game code couldn't be recognized from the directory. Please use the correct game code as the save directory name.")
        };
    }

    private static byte[]? ComputeTigerHash(byte[] data)
    {
        var tiger = new Tiger();
        tiger.ComputeHash(data);
        return tiger.Hash;
    }

    private static string ByteArrayToString(byte[] ba)
    {
        var hex = new StringBuilder(ba.Length * 2);
        foreach (var b in ba)
            hex.Append($"{b:X2}");
        return hex.ToString();
    }

    private static uint Checksum(byte[] data)
    {
        var result = ~Hash.Hash.CRC32_0x77073096(data);
        return result;
    }

}