namespace dsi.common.Models
{
    /// <summary>
    /// Информация о данных содержащихся в классе FolderNode
    /// </summary>
    public class FolderNodeInfo
    {
        public FolderNodeInfo(string fullName, DateTime creationDate)
        {
            FullName = fullName;
            CreationDate = creationDate;
        }

        public string FullName { get; private set; }

        public DateTime CreationDate { get; private set; }

    }
}
