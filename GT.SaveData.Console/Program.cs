using System.Diagnostics;
using System.Reflection;
using PS3FileSystem;

namespace GT.SaveData.Console;

class Program
{

    private static void PrintInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        string? version = fvi.FileVersion;

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

        Game game;
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

            game = DetectGame(args[0]);

            System.Console.Write("Select the operation decrypt/encrypt (d or e): ");
            string? response = System.Console.ReadLine();
            switch (response)
            {
                case "d":
                    args = ["d", args[0]];
                    break;
                case "e":
                    args = ["e", args[0]];
                    break;
                default:
                    System.Console.WriteLine("Invalid option, expected d for decrypt or e for encrypt.");
                    System.Console.WriteLine();
                    System.Console.WriteLine("Press any key to exit...");
                    System.Console.ReadKey();
                    return;
            }
        }
        else
        {
            game = DetectGame(args[1]);
        }

        if (args[0].Equals("d", StringComparison.OrdinalIgnoreCase) ||
            args[0].Equals("--decrypt", StringComparison.OrdinalIgnoreCase))
        {
            new Unpacker(args[1], game).Decrypt();
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

#if !DEBUG
        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();
#endif
    }

    private static Game DetectGame(string folder)
    {
        var game = DetectGameFromSfo(folder);
        if (game != null)
        {
            System.Console.WriteLine($"Game detected from PARAM.SFO: {game}");
            return game.Value;
        }

        game = DetectGameFromDirectory(folder);
        if (game != null)
        {
            System.Console.WriteLine($"Game detected from directory: {game}");
            return game.Value;
        }

        // Ask the user to select the game
        System.Console.WriteLine("Game code couldn't be recognized. Please select the game:");
        System.Console.WriteLine("1. GT5");
        System.Console.WriteLine("2. GT6");
        System.Console.WriteLine("3. GT6 GameScom Demo");
        System.Console.WriteLine("4. GT5 Prologue");
        System.Console.WriteLine("5. GT5 Time Trial Challenge");
        System.Console.WriteLine("6. GT HD Concept");
        System.Console.Write("Select the game (1-5): ");
        string? response = System.Console.ReadLine();
        return response switch
        {
            "1" => Game.GT5,
            "2" => Game.GT6,
            "3" => Game.GT6GC,
            "4" => Game.GT5P,
            "5" => Game.GT5TTC,
            "6" => Game.GTHD,
            _ => throw new NotImplementedException(
                "Game code couldn't be recognized. Please ensure you have the correct game code as the save directory name.")
        };
    }

    private static Game? DetectGameFromSfo(string folder)
    {
        string sfoPath = Path.Combine(folder, "PARAM.SFO");
        if (!File.Exists(sfoPath))
        {
            System.Console.WriteLine("PARAM.SFO not found.");
            return null;
        }

        var sfo = new PARAM_SFO(sfoPath);
        return GameMapper.GetGameByTitleId(sfo.TitleID);
    }

    private static Game? DetectGameFromDirectory(string folder)
    {
        string gameCode = Path.GetFileName(folder);
        return gameCode switch
        {
            "NPEA90002-GAME-" => Game.GTHD,
            "NPJA90061-GAME-" or "BCJS30017-GAME-" => Game.GT5P,
            "NPHA80080-GAME-" or "NPUA70087-GAME-" or "NPEA90052-GAME-" => Game.GT5TTC,
            "BCJS37016-GAME6" or "BCJS37016-BKUP6" => Game.GT6,
            "DEMO32768-GAME6" or "DEMO32768-BKUP6" => Game.GT6GC,
            _ => null
        };
    }

}
