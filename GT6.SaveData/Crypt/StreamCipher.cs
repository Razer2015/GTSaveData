namespace GT.SaveData.Crypt
{
    public static class StreamCipher
    {

        public static int Cipher(ref int cipher)
        {
            cipher += (cipher << 0x4) + 0x11;

            var bitReverse = BitReverse((uint)cipher);

            var or = bitReverse << 0x18 | (bitReverse & 0xFF00) << 0x8 | bitReverse >> 0x18 |
                     (bitReverse >> 0x8) & 0xFF00;
            var shifted = or << 0x8;
            shifted += or;
            shifted += 0x101;

            return (int)(or ^ RotateRight(shifted, 0x10));
        }

        private static uint BitReverse(uint value)
        {
            var left = (uint)1 << 31;
            uint right = 1;
            uint result = 0;

            for (var i = 31; i >= 1; i -= 2)
            {
                result |= (value & left) >> i;
                result |= (value & right) << i;
                left >>= 1;
                right <<= 1;
            }

            return result;
        }

        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        private static uint RotateRight(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }

    }
}