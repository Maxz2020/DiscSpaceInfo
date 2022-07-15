using dsi.common.Interfaces;
using dsi.common.Models;

namespace dsi.services
{
    public class NodeComparer : INodeComparer
    {
        private const int FilesToDictionary = 10;
        private volatile uint _filesProcessed;

        public uint GetFilesProcessed()
        {
            return _filesProcessed;
        }

        public async Task<FolderNodeChangesContainer> GetChangesAsync(FolderNode oldFolderNode, FolderNode newFolderNode, CancellationToken cancellationToken)
        {
            _filesProcessed = 0;
            var nodeChanges = new FolderNodeChanges(newFolderNode);
            var changesContainer = new FolderNodeChangesContainer(nodeChanges, oldFolderNode.CreationDate, newFolderNode.CreationDate);

            return await Task<FolderNodeChangesContainer>.Factory.StartNew(() => StartCompare(oldFolderNode, changesContainer), cancellationToken);
        }

        public FolderNodeChangesContainer GetChanges(FolderNode oldFolderNode, FolderNode newFolderNode)
        {
            _filesProcessed = 0;
            var nodeChanges = new FolderNodeChanges(newFolderNode);
            var changesContainer = new FolderNodeChangesContainer(nodeChanges, oldFolderNode.CreationDate, newFolderNode.CreationDate);

            StartCompare(oldFolderNode, changesContainer);
            return changesContainer;
        }


        private FolderNodeChangesContainer StartCompare(FolderNode oldFolderNode, FolderNodeChangesContainer changesContainer)
        {
            GetChanges(oldFolderNode, changesContainer.FolderNodeChanges);
            return changesContainer;
        }

        private void GetChanges(FolderNode oldFolderNode, FolderNodeChanges nodeChanges)
        {
            GetNotExistedFiles(oldFolderNode, nodeChanges);

            GetDeletedFiles(oldFolderNode, nodeChanges);

            foreach (var oldFolder in oldFolderNode.Folders)
            {
                var newFolder = nodeChanges.Folders.Find(_ => string.Equals(_.FullName, oldFolder.FullName, StringComparison.Ordinal));

                if (newFolder == null)
                {
                    nodeChanges.Folders.Add(new FolderNodeChanges(oldFolder, true));
                }
            }

            foreach (var folder in nodeChanges.Folders)
            {
                var oldFolder = oldFolderNode.Folders.Find(_ => string.Equals(_.FullName, folder.FullName, StringComparison.Ordinal));

                if (oldFolder == null)
                {
                    folder.IsNew = true;
                }
                else
                {
                    GetChanges(oldFolder, folder);
                }
            }
        }

        private void GetNotExistedFiles(FolderNode oldFolderNode, FolderNodeChanges nodeChanges)
        {
            var oldFolderNodeFiles = new Dictionary<string, FileNode>();
            bool useDict = false;
            if (oldFolderNode.Files.Count > FilesToDictionary)
            {
                useDict = true;
                foreach (var file in oldFolderNode.Files)
                {
                    oldFolderNodeFiles.Add(file.Name, file);
                }
            }

            foreach (var file in nodeChanges.Files)
            {
                FileNode? oldFile;
                if (useDict)
                {
                    _ = oldFolderNodeFiles.TryGetValue(file.Name, out oldFile);
                }
                else
                {
                    oldFile = oldFolderNode.Files.Find(_ => string.Equals(_.FullName, file.FullName, StringComparison.Ordinal));
                }

                if (oldFile == null)
                {
                    file.IsNew = true;
                }
                else
                {
                    file.CrcOld = oldFile.Crc;
                    file.CreationTimeOld = oldFile.CreationTime;
                    file.LastWriteTimeOld = oldFile.LastWriteTime;
                    file.LengthOld = oldFile.Length;
                }
                _filesProcessed++;
            }
        }

        private void GetDeletedFiles(FolderNode oldFolderNode, FolderNodeChanges nodeChanges)
        {
            var nodeChangesFiles = new Dictionary<string, FileNode>();
            var useDict = false;
            if (nodeChanges.Files.Count > FilesToDictionary)
            {
                useDict = true;
                foreach (var file in nodeChanges.Files)
                {
                    nodeChangesFiles.Add(file.Name, file);
                }
            }

            foreach (var oldFile in oldFolderNode.Files)
            {
                FileNode? newFile;
                if (useDict)
                {
                    _ = nodeChangesFiles.TryGetValue(oldFile.Name, out newFile);
                }
                else
                {
                    newFile = nodeChanges.Files.Find(_ => string.Equals(_.FullName, oldFile.FullName, StringComparison.Ordinal));
                }

                if (newFile == null)
                {
                    nodeChanges.Files.Add(new FileNodeChanges(oldFile, true));
                }
                _filesProcessed++;
            }
        }
    }
}
