using MessagePack;

namespace dsi.common.Models
{
    /// <summary>
    /// Информация об изменениях в дереве каталогов
    /// </summary>
    public class FolderNodeChanges
    {
        public FolderNodeChanges(FolderNode folderNode, bool isDeleted = false)
        {
            FullName = folderNode.FullName;
            IsDeleted = isDeleted;

            Folders = new List<FolderNodeChanges>();
            foreach (var folder in folderNode.Folders)
            {
                Folders.Add(new FolderNodeChanges(folder, isDeleted));
            }

            Files = new List<FileNodeChanges>();
            foreach (var file in folderNode.Files)
            {
                var fileNodeChanges = new FileNodeChanges(file)
                {
                    IsDeleted = isDeleted
                };
                Files.Add(fileNodeChanges);
            }
        }

        public string? FullName { get; set; }

        public List<FileNodeChanges> Files { get; set; }

        public List<FolderNodeChanges> Folders { get; set; }

        private bool isNew;
        public bool IsNew
        {
            get => isNew;

            set
            {
                isNew = value;
                foreach (var file in Files)
                {
                    file.IsNew = value;
                }

                foreach (var folder in Folders)
                {
                    folder.IsNew = value;
                }
            }
        }

        public bool IsDeleted { get; set; }

        [IgnoreMember]
        public bool IsFolderChanged => IsNew || IsDeleted;

        /// <summary>
        /// Общее кол-во файлов
        /// </summary>
        /// <param name="countNew">Считать новые</param>
        /// <param name="countDeleted">Считать удалённые</param>
        /// <returns></returns>
        public long GetTotalFilesCount(bool countNew, bool countDeleted)
        {
            long result = 0;

            result += Files.Where(x => !x.IsNew && !x.IsDeleted || x.IsNew && countNew || x.IsDeleted && countDeleted).Count();

            foreach (var folder in Folders)
            {
                result += folder.GetTotalFilesCount(countNew, countDeleted);
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
    }
}
