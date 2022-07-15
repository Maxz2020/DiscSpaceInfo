namespace dsi.services.Crc
{
    /// <summary>
    /// (c) Из просторов
    /// </summary>
    public static class Crc8
    {
        private static readonly byte[] Table = new byte[256];
        private const byte Poly = 0xd5;

        public static byte ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes is { Length: > 0 }) crc = bytes.Aggregate(crc, (current, b) => Table[current ^ b]);
            return crc;
        }

        static Crc8()
        {
            for (var i = 0; i < 256; ++i)
            {
                var temp = i;
                for (var j = 0; j < 8; ++j)
                    if ((temp & 0x80) != 0)
                        temp = (temp << 1) ^ Poly;
                    else
                        temp <<= 1;
                Table[i] = (byte)temp;
            }
        }
    }
}
