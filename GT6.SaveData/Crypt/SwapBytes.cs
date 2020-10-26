namespace GT.SaveData.Crypt {
    public static class SwapBytes {
        public static byte[] ByteSwap(byte[] data) {
            var offset1 = GetOffset2(data.Length, 0.753459f);
            var offset2 = GetOffset2(data.Length - 0x04, 0.262591f) + 0x04;

            data = Swap(data, 0x00, offset1);
            return Swap(data, 0x04, offset2);
        }

        private static int GetOffset2(int length, float multiplier) {
            return (int)((length - 0x04) * multiplier);
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
