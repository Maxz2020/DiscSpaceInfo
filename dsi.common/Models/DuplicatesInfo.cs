namespace dsi.common.Models
{
    /// <summary>
    /// Инфорация о дубликатах файлов
    /// </summary>
    public class DuplicatesInfo
    {
        public DuplicatesInfo() => Files = new List<FileNode>();

        public List<FileNode> Files { get; set; }
    }
}
