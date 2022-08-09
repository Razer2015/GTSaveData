using System;

namespace GT.SaveData.Crypt {
    public static class SwapBytes {
        public static byte[] ByteSwap(byte[] data, Game game) {
            //var (multiplier1, multiplier2) = GetMultipliers(game);
            //var offset1 = GetOffset(data.Length, multiplier1);
            //var offset2 = GetOffset(data.Length - 0x04, multiplier2) + 0x04;
            var (offset1, offset2) = GetOffsets(data.Length, game);

            data = Swap(data, 0x00, offset1);
            return Swap(data, 0x04, offset2);
        }

        private static (float Multiplier1, float Multiplier2) GetMultipliers(Game game) {
            switch (game) {
                case Game.GTHD:
                case Game.GTPSP:
                case Game.GT5P:
                case Game.GT5TTC:
                    return (0.858477f, 0.327032f);
                case Game.GT6GC:
                case Game.GT6:
                    return (0.753459f, 0.262591f); // These seem to be having issues with some saves
                default:
                    throw new ArgumentOutOfRangeException($"Argument out of range in {nameof(GetMultipliers)} for {nameof(game)}");
            }
        }

        private static int GetOffset(int length, float multiplier) {
            return (int)((length - 0x04) * multiplier);
        }

        private static int GetOffset(int length, uint multiplier) {
            return (int)(((length - 0x04) * multiplier) >> 32);
        }

        private static (int Offset1, int Offset2) GetOffsets(int length, Game game) {
            switch (game) {
                case Game.GTHD:
                case Game.GTPSP:
                case Game.GT5P:
                case Game.GT5TTC:
                    return (GetOffset(length, 0.858477f), GetOffset(length - 0x04, 0.327032f) + 0x04);
                case Game.GT6GC:
                case Game.GT6:
                    //return (GetOffset(length, 0.753459f), GetOffset(length - 0x04, 0.262591f) + 0x04);
                    return (GetOffset(length, 0xC0E2BE6E), GetOffset(length - 0x04, 0x4339334B) + 0x04);
                default:
                    throw new ArgumentOutOfRangeException($"Argument out of range in {nameof(GetOffsets)} for {nameof(game)}");
            }
            
        }

        private static byte[] Swap(byte[] data, long offset1, long offset2) {
            var save1 = data[offset1];
            var save2 = data[offset1 + 1];
            var save3 = data[offset1 + 2];
            var save4 = data[offset1 + 3];

            data[offset1] = data[offset2];
            data[offset1 + 1] = data[offset2 + 1];
            data[offset1 + 2] = data[offset2 + 2];
            data[offset1 + 3] = data[offset2 + 3];

            data[offset2] = save1;
            data[offset2 + 1] = save2;
            data[offset2 + 2] = save3;
            data[offset2 + 3] = save4;

            return data;
        }
    }
}
