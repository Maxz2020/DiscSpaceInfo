namespace dsi.services.Crc
{
    /// <summary>
    /// (c) Из просторов
    /// </summary>
    public class Crc16
    {
        public enum Crc16Mode : ushort
        {
            Standard = 0xA001,
            CcittKermit = 0x8408
        }

        private readonly ushort[] _table = new ushort[256];

        private ushort ComputeChecksum(params byte[] bytes)
        {
            return bytes.Aggregate<byte, ushort>(0, (current, t) => (ushort)(current >> 8 ^ _table[(byte)(current ^ t)]));
        }

        public byte[] ComputeChecksumBytes(params byte[] bytes)
        {
            var crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public Crc16(Crc16Mode mode)
        {
            var polynomial = (ushort)mode;
            for (ushort i = 0; i < _table.Length; ++i)
            {
                ushort value = 0;
                var temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                        value = (ushort)(value >> 1 ^ polynomial);
                    else
                        value >>= 1;
                    temp >>= 1;
                }

                _table[i] = value;
            }
        }
    }
}
