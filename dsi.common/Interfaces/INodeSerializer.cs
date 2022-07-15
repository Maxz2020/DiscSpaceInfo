using dsi.common.Models;

namespace dsi.common.Interfaces
{
    /// <summary>
    /// Сериализация дерева каталогов
    /// </summary>
    public interface INodeSerializer
    {
        Task<byte[]> SerializeAsync(FolderNode node, CancellationToken cancellationToken);

        Task<FolderNode> DeserializeAsync(byte[] compressedBytes, CancellationToken cancellationToken);

        FolderNode Deserialize(byte[] compressedNode);

        Task<HeaderData> DeserializeHeaderAdditionalDataAsync(byte[] compressedNode, CancellationToken cancellationToken);
    }
}
