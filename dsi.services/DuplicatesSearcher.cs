using dsi.common.Interfaces;
using dsi.common.Models;

namespace dsi.services
{
    public class DuplicatesSearcher : IDuplicatesSearcher
    {
        private const long _minFileLength = 1;

        public List<DuplicatesInfo> GetDuplicates(FolderNode folder)
        {
            return GetDuplicates(new List<FolderNode>() { folder });
        }

        public List<DuplicatesInfo> GetDuplicates(List<FolderNode> folders)
        {
            List<DuplicatesInfo> result = new();
            Dictionary<string, List<FileNode>> allFiles = new();

            foreach (var folder in folders)
            {
                foreach (var file in folder.GetAllFiles())
                {
                    if (file.Length >= _minFileLength)
                    {
                        var keyString = $"{file.Length}_{file.Crc}";

                        allFiles.TryGetValue(keyString, out var fileList);
                        if (fileList != null)
                        {
                            fileList.Add(file);
                        }
                        else
                        {
                            allFiles.Add(keyString, new List<FileNode>() { file });
                        }
                    }
                }
            }

            foreach (var keyValuePair in allFiles)
            {
                if (keyValuePair.Value.Count > 1)
                {
                    result.Add(new DuplicatesInfo()
                    {
                        Files = keyValuePair.Value
                    });
                }
            }

            return result;
        }
    }
}
