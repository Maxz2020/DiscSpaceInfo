using dsi.common.Interfaces;
using dsi.services.Crc;

namespace dsi.services
{
    public class CrcProvider : ICrcProvider
    {
        private const string CALC_CRC = "CalcCrc";
        private readonly bool _calculateCrc = false;
        private readonly ILogger _logger;

        public CrcProvider(IConfigProvider configProvider, ILogger logger)
        {
            var calculateCrcStr = configProvider.GetValue(CALC_CRC);

            _ = bool.TryParse(calculateCrcStr, out _calculateCrc);

            _logger = logger;
        }

        public uint GetFileChecksum(string filename)
        {
            if (!_calculateCrc)
            {
                return 0;
            }

            Crc32 crcCounter = new();
            uint result = 0;
            try
            {
                var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                result = crcCounter.ComputeChecksum(StreamAsIEnumerable(stream));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }

            return result;
        }

        private static IEnumerable<byte> StreamAsIEnumerable(Stream stream)
        {
            for (int i = stream.ReadByte(); i != -1; i = stream.ReadByte())
            {
                yield return (byte)i;
            }
        }
    }
}
