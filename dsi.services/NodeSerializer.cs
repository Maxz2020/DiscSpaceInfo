using dsi.common.Interfaces;
using dsi.common.Models;
using MessagePack;

namespace dsi.services
{
    public class NodeSerializer : INodeSerializer
    {
        private const string TypeCodeString = "dsi";
        private const ushort CurrentCoderVersion = 1;
        private readonly ushort[] SupportedVersions = new ushort[] { 1 };
        private const ushort StaticHeaderSize = 7;
        private const string NodeFullNameKey = "NodeFullName";
        private const string NodeSaveDateKey = "NodeSaveDate";

        private readonly MessagePackSerializerOptions _lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        private readonly ILogger _logger;

        public NodeSerializer(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> SerializeAsync(FolderNode node, CancellationToken cancellationToken)
        {

            await using var stream = new MemoryStream();
            cancellationToken.ThrowIfCancellationRequested();
            await MessagePackSerializer.SerializeAsync(stream, node, _lz4Options, cancellationToken);

            var headerAddData = new HeaderData();
            headerAddData.keyValuePairs.Add(NodeFullNameKey, node.FullName);
            headerAddData.keyValuePairs.Add(NodeSaveDateKey, node.CreationDate);
            var compressedHeaderAddData = await SerializeHeaderAdditionalData(headerAddData, cancellationToken);

            return AddHeader(stream.ToArray(), compressedHeaderAddData);
        }

        public async Task<FolderNode> DeserializeAsync(byte[] compressedNode, CancellationToken cancellationToken)
        {
            compressedNode = GetSerializedNode(compressedNode);
            try
            {
                return await Task.Run(() => MessagePackSerializer.Deserialize<FolderNode>(compressedNode, _lz4Options), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        public FolderNode Deserialize(byte[] compressedNode)
        {
            compressedNode = GetSerializedNode(compressedNode);
            try
            {
                using var stream = new MemoryStream();

                return MessagePackSerializer.Deserialize<FolderNode>(compressedNode, _lz4Options);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        public async Task<HeaderData> DeserializeHeaderAdditionalDataAsync(byte[] compressedNode, CancellationToken cancellationToken)
        {
            compressedNode = GetSerializedHeaderAdditionalData(compressedNode);
            
            if (compressedNode.Length == 0)
            {
                return new HeaderData();
            }

            try
            {
                return await Task.Run(() => MessagePackSerializer.Deserialize<HeaderData>(compressedNode, _lz4Options), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        private async Task<byte[]> SerializeHeaderAdditionalData(HeaderData headerData, CancellationToken cancellationToken)
        {
            using var headerStream = new MemoryStream();
            await MessagePackSerializer.SerializeAsync(headerStream, headerData, _lz4Options, cancellationToken);

            return await Task.Run(() => headerStream.ToArray()).ConfigureAwait(false);
        }

        private static byte[] AddHeader(byte[] compressedNode, byte[] compressedHeaderAddData)
        {
            var idBytes = TypeCodeString.ToArray();
            var versionBytes = BitConverter.GetBytes(CurrentCoderVersion);

            ushort headerSize = (ushort)(StaticHeaderSize + (ushort)compressedHeaderAddData.Length);
            var headerSizeBytes = BitConverter.GetBytes(headerSize);

            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(idBytes);
                writer.Write(versionBytes);
                writer.Write(headerSizeBytes);
                writer.Write(compressedHeaderAddData);
                writer.Write(compressedNode);
            }

            return stream.ToArray();
        }

        private void CheckHeader(byte[] compressedNode)
        {
            var headerBytes = new char[3];
            Array.Copy(compressedNode, headerBytes, 3);
            var header = new string(headerBytes);
            if (header != TypeCodeString)
            {
                throw new FormatException($"Указанный файл не является файлом {TypeCodeString}.");
            }

            var versionBytes = new byte[2];
            Array.Copy(compressedNode, 3, versionBytes, 0, 2);
            var version = BitConverter.ToUInt16(versionBytes);
            if (!SupportedVersions.Contains(version))
            {
                throw new FormatException($"Версия {version} файла {TypeCodeString} не поддерживается.");
            }
        }

        private byte[] GetSerializedNode(byte[] compressedNode)
        {
            CheckHeader(compressedNode);

            var headerSizeBytes = new byte[2];
            Array.Copy(compressedNode, 5, headerSizeBytes, 0, 2);
            var headerSize = BitConverter.ToUInt16(headerSizeBytes);

            var resultLength = compressedNode.Length - headerSize;
            var result = new byte[resultLength];
            Array.Copy(compressedNode, headerSize, result, 0, resultLength);
            return result;
        }

        private byte[] GetSerializedHeaderAdditionalData(byte[] compressedNode)
        {
            CheckHeader(compressedNode);

            var headerSizeBytes = new byte[2];
            Array.Copy(compressedNode, 5, headerSizeBytes, 0, 2);
            var headerSize = BitConverter.ToUInt16(headerSizeBytes);

            var resultLength = headerSize - StaticHeaderSize;
            var result = new byte[resultLength];

            Array.Copy(compressedNode, StaticHeaderSize, result, 0, resultLength);
            return result;
        }
    }
}
