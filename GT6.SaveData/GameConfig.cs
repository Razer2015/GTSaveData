using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GT.SaveData {
    public class GameConfigModel {
        public string Notice { get; set; }
        public SupportedGameModel[] SupportedGames { get; set; }
        public GameModel[] Games { get; set; }

        public GameConfigModel(GameModel[] games) {
            Notice = $"{nameof(SupportedGames)} node doesn't support editing. It's just for showing which games are supported.";
            SupportedGames = Enum.GetValues(typeof(Game))
                .Cast<Game>()
                .Select(x => new SupportedGameModel(x.ToString(), (int)x))
                .ToArray();
            Games = games;
        }
    }

    public class SupportedGameModel {
        public string Game { get; set; }
        public int Index { get; set; }

        public SupportedGameModel(string game, int index) {
            Game = game;
            Index = index;
        }
    }

    public class GameModel {
        public string GameCode { get; set; }
        public Game Game { get; set; }

        public GameModel(string gameCode, Game game) {
            GameCode = gameCode;
            Game = game;
        }
    }

    public class GameConfig {
        private const string CONFIG_NAME = "games.json";
        private readonly string _directory;
        private GameConfigModel? _gameConfig;
        private Game? _game { get; set; }

        public GameConfig(string dir) {
            _directory = dir;
        }

        public GameConfig(Game game)
        {
            _game = game;
        }

        public Game DetermineGame() {
            if (_game != null) return _game.Value;

            if (File.Exists(CONFIG_NAME)) {
                try {
                    _gameConfig = JsonConvert.DeserializeObject<GameConfigModel>(File.ReadAllText(CONFIG_NAME));
                }
                catch (Exception) {
                    Console.WriteLine("Invalid game config, ignoring...");
                }
            }
            else {
                _gameConfig = new GameConfigModel(new[] {
                    new GameModel("NPEA90002", Game.GTHD),
                    new GameModel("NPUA80019", Game.GTHD),
                    new GameModel("BCES00104", Game.GT5P),
                    new GameModel("BCJS30017", Game.GT5P),
                    new GameModel("BCJS30031", Game.GT5P),
                    new GameModel("BCUS98158", Game.GT5P),
                    new GameModel("NPUA80075", Game.GT5P),
                    new GameModel("NPJA90061", Game.GT5P),
                    new GameModel("NPHA80080", Game.GT5TTC),
                    new GameModel("NPUA70087", Game.GT5TTC),
                    new GameModel("NPEA90052", Game.GT5TTC),
                    new GameModel("BCJS37016", Game.GT6),
                    new GameModel("DEMO32768", Game.GT6GC)
                });

                File.WriteAllText(CONFIG_NAME, JsonConvert.SerializeObject(_gameConfig, Formatting.Indented));
            }

            string? gameCode = Path.GetFileName(_directory)
                .Split('-')
                .FirstOrDefault();

            var match = _gameConfig?.Games.FirstOrDefault(x => x.GameCode.Equals(gameCode, StringComparison.OrdinalIgnoreCase));
            if (match == default)
            {
                throw new ArgumentOutOfRangeException("Couldn't determine the game version.");
            }

            _game = match.Game;
            return match.Game;

        }

        public void UpdateGame(Game game) {
            _game = game;
            SaveGame();
        }

        public void SaveGame() {
            File.WriteAllText(Path.Combine(_directory, CONFIG_NAME), JsonConvert.SerializeObject(_game));
        }
    }
}
