namespace dsi.services.Crc
{
    public class Crc16Ccitt
    {
        /// <summary>
        /// (c) Из просторов
        /// </summary>
        public enum InitialCrcValue
        {
            Zeros,
            NonZero1 = 0xffff,
            NonZero2 = 0x1D0F
        }

        private const ushort Poly = 4129;
        private readonly ushort[] _table = new ushort[256];
        private readonly ushort _initialValue;

        private ushort ComputeChecksum(IEnumerable<byte> bytes)
        {
            return bytes.Aggregate(_initialValue, (current, t) => (ushort)((current << 8) ^ _table[(current >> 8) ^ (0xff & t)]));
        }

        public byte[] ComputeChecksumBytes(IEnumerable<byte> bytes)
        {
            var crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public Crc16Ccitt(InitialCrcValue initialValue)
        {
            _initialValue = (ushort)initialValue;
            for (var i = 0; i < _table.Length; ++i)
            {
                ushort temp = 0;
                var a = (ushort)(i << 8);
                for (var j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                        temp = (ushort)((temp << 1) ^ Poly);
                    else
                        temp <<= 1;
                    a <<= 1;
                }

                _table[i] = temp;
            }
        }
    }
}
