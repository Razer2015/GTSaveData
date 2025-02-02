using System;
using System.IO;
using System.Linq;
using PS3FileSystem;

namespace GT.SaveData.Crypt
{
    public class SonyCrypt
    {
        private readonly Game _game;

        public SonyCrypt(Game game)
        {
            _game = game;
        }

        public void Decrypt(string path)
        {
            Ps3SaveManager manager = new Ps3SaveManager(path, GetKey());
            manager.DecryptAllFiles();
        }

        public byte[]? DecryptFileToBytes(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory == null) return null;
            var manager = new Ps3SaveManager(directory, GetKey());

            var file = manager.Files.FirstOrDefault(x => x.FilePath == filePath);

            if (file == null) return null;

            return !file.IsEncrypted ? file.GetBytes() : file?.DecryptToBytes();
        }

        public Ps3SaveManager Load(string path)
        {
            return new Ps3SaveManager(path, GetKey());
        }

        public Ps3File[] GetFiles(string path)
        {
            var manager = new Ps3SaveManager(path, GetKey());
            return manager.Files;
        }

        public void Encrypt(string path)
        {
            var manager = new Ps3SaveManager(path, GetKey());
            manager.ReBuildChanges(true);
        }

        public void Rebuild(string path)
        {
            var manager = new Ps3SaveManager(path, GetKey());
            manager.ReBuildChanges(false);
        }

        private byte[] GetKey()
        {
            switch (_game)
            {
                case Game.GT6:
                    return
                    [
                        0x77, 0x1D, 0x1C, 0x71, 0xE7, 0x5B, 0x4E, 0x70, 0x80, 0x38, 0x73, 0xF7, 0x40, 0x25, 0x11, 0xA7
                    ];
                case Game.GT5:
                    return
                    [
                        0xBD, 0xBD, 0x2E, 0xB7, 0x2D, 0x82, 0x47, 0x3D, 0xBE, 0x09, 0xF1, 0xB5, 0x52, 0xA9, 0x3F, 0xE6
                    ];
                case Game.GTHD:
                case Game.GTPSP:
                case Game.GT5P:
                case Game.GT5TTC:
                case Game.GT6GC:
                default:
                    throw new ArgumentOutOfRangeException($"The game {_game} isn't supported.");
            }
        }

    }
}
