using dsi.common.Models;

namespace dsi.common.Interfaces
{
    /// <summary>
    /// Сравнение деревьев каталогов
    /// </summary>
    public interface INodeComparer
    {
        public uint GetFilesProcessed();

        Task<FolderNodeChangesContainer> GetChangesAsync(FolderNode oldFolderNode, FolderNode newFolderNode, CancellationToken cancellationToken);

        FolderNodeChangesContainer GetChanges(FolderNode oldFolderNode, FolderNode newFolderNode);
    }
}
