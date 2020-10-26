using System;
using System.Collections.Generic;
using System.Text;
using PS3FileSystem;

namespace GT.SaveData.GT6 {
    public class SonyCrypt {
        private readonly Game _game;

        public SonyCrypt(Game game) {
            _game = game;
        }

        public void Decrypt(string path) {
            Ps3SaveManager manager = new Ps3SaveManager(path, GetKey());
            manager.DecryptAllFiles();
        }

        public void Load(string path) {
            Ps3SaveManager manager = new Ps3SaveManager(path, GetKey());
        }

        public Ps3File[] GetFiles(string path) {
            Ps3SaveManager manager = new Ps3SaveManager(path, GetKey());
            return manager.Files;
        }

        public void Encrypt(string path) {
            var manager = new Ps3SaveManager(path, GetKey());
            manager.ReBuildChanges(true);
        }

        public void Rebuild(string path) {
            var manager = new Ps3SaveManager(path, GetKey());
            manager.ReBuildChanges(false);
        }

        private byte[] GetKey() {
            switch (_game) {
                case Game.GT6:
                    return new byte[] { 0x77, 0x1D, 0x1C, 0x71, 0xE7, 0x5B, 0x4E, 0x70, 0x80, 0x38, 0x73, 0xF7, 0x40, 0x25, 0x11, 0xA7 };
                case Game.GT5:
                    return new byte[] { 0xBD, 0xBD, 0x2E, 0xB7, 0x2D, 0x82, 0x47, 0x3D, 0xBE, 0x09, 0xF1, 0xB5, 0x52, 0xA9, 0x3F, 0xE6 };
                case Game.GTHD:
                case Game.GTPSP:
                case Game.GT5P:
                case Game.GT5TTC:
                default:
                    throw new ArgumentOutOfRangeException($"The game {_game} isn't supported.");
            }
        }
    }
}
