using MessagePack;

namespace dsi.common.Models
{
    /// <summary>
    /// Информация о папке в дереве каталогов
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class FolderNode : Node
    {
        public FolderNode(DateTime creationDate) : this()
        {
            CreationDate = creationDate;
        }

        public FolderNode()
        {
            Files = new List<FileNode>();
            Folders = new List<FolderNode>();
        }

        public DateTime CreationDate { get; set; }

        public List<FileNode> Files { get; set; }

        public List<FolderNode> Folders { get; set; }

        /// <summary>
        /// Получить общее кол-во файлов
        /// </summary>
        /// <returns></returns>
        public long GetTotalFilesCount()
        {
            long result = 0;

            result += Files.Count;

            foreach (var folder in Folders)
            {
                result += folder.GetTotalFilesCount();
            }

            return result;
        }

        /// <summary>
        /// Получить общее кол-во каталогов
        /// </summary>
        /// <returns></returns>
        public long GetTotalFoldersCount()
        {
            long result = 0;

            result += Folders.Count;

            foreach (var folder in Folders)
            {
                result += folder.GetTotalFoldersCount();
            }

            return result;
        }

        /// <summary>
        /// Получить общий размер
        /// </summary>
        /// <returns></returns>
        public long GetTotalLength()
        {
            long result = 0;

            result += Files.Sum(_ => _.Length);

            foreach (var folder in Folders)
            {
                result += folder.GetTotalLength();
            }

            return result;
        }

        /// <summary>
        /// Получить все файлы
        /// </summary>
        /// <returns></returns>
        public List<FileNode> GetAllFiles()
        {
            List<FileNode> result = new();

            result.AddRange(Files);

            foreach (var folder in Folders)
            {
                result.AddRange(folder.GetAllFiles());
            }

            return result;
        }
    }
}
