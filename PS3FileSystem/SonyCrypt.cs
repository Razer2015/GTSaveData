namespace PS3FileSystem {
    public static class SonyCrypt {
        static readonly byte[] Key = new byte[] { 0xBD, 0xBD, 0x2E, 0xB7, 0x2D, 0x82, 0x47, 0x3D, 0xBE, 0x09, 0xF1, 0xB5, 0x52, 0xA9, 0x3F, 0xE6 };

        public static void Decrypt(string path) {
            Ps3SaveManager manager = new Ps3SaveManager(path, Key);
            manager.DecryptAllFiles();
        }

        public static void Load(string path) {
            Ps3SaveManager manager = new Ps3SaveManager(path, Key);
        }

        public static Ps3File[] GetFiles(string path) {
            Ps3SaveManager manager = new Ps3SaveManager(path, Key);
            return manager.Files;
        }

        public static void Encrypt(string path) {
            var manager = new Ps3SaveManager(path, Key);
            manager.ReBuildChanges(true);
        }

        public static void Rebuild(string path) {
            var manager = new Ps3SaveManager(path, Key);
            manager.ReBuildChanges(false);
        }
    }
}
