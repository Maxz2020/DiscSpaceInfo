using dsi.common.Models;

namespace dsi.common.Interfaces
{
    /// <summary>
    /// ПОлучение инфорации о дубликатах файлов 
    /// </summary>
    public interface IDuplicatesSearcher
    {
        /// <summary>
        /// Инфорация о дубликатах файлов в списке каталогов
        /// </summary>
        /// <param name="folders"></param>
        /// <returns></returns>
        public List<DuplicatesInfo> GetDuplicates(List<FolderNode> folders);

        /// <summary>
        /// Инфорация о дубликатах файлов в каталоге
        /// </summary>
        /// <param name="folders"></param>
        /// <returns></returns>
        public List<DuplicatesInfo> GetDuplicates(FolderNode folder);
    }
}
