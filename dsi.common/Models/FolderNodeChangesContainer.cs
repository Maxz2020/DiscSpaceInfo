namespace dsi.common.Models
{
    /// <summary>
    /// Контейнер информации об изменениях в дереве каталогов
    /// </summary>
    public class FolderNodeChangesContainer
    {
        public FolderNodeChangesContainer(FolderNodeChanges folderNodeChanges, DateTime startChanges, DateTime endChanges)
        {
            FolderNodeChanges = folderNodeChanges;
            StartChangesDate = startChanges;
            EndChangesDate = endChanges;
        }

        public FolderNodeChanges FolderNodeChanges { get; private set; }

        public DateTime StartChangesDate { get; private set; }

        public DateTime EndChangesDate { get; private set; }

        public string? StartFileName { get; set; }

        public string? EndFileName { get; set; }
    }
}
