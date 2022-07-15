using dsi.common.Models;

namespace dsi.common.Interfaces
{
    /// <summary>
    /// Чтение дерева каталогов
    /// </summary>
    public interface INodeReader
    {
        public uint GetFilesProcessed();

        FolderNode Create(string fullPath);

        Task<FolderNode> CreateAsync(string fullPath, CancellationToken cancellationToken);
    }
}
