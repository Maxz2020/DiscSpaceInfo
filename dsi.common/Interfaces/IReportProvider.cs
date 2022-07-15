using dsi.common.Models;

namespace dsi.common.Interfaces
{
    /// <summary>
    /// Получение отчета
    /// </summary>
    public interface IReportProvider
    {
        /// <summary>
        /// Получить отчет о дубликатах
        /// </summary>
        /// <param name="duplicates"></param>
        /// <returns></returns>
        List<string> GetReport(IEnumerable<DuplicatesInfo>? duplicates);

        /// <summary>
        /// Получить отчет об изменениях
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        List<string> GetReport(FolderNodeChangesContainer? changes);
    }
}
