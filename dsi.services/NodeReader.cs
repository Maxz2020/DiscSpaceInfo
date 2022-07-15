using dsi.common.Interfaces;
using dsi.common.Models;

namespace dsi.services
{
    public class NodeReader : INodeReader
    {
        private const string CacheKeyPrefix = "CRC_";

        readonly ICrcProvider? _crcProvider;
        private readonly IChacheServise _chacheServise;

        private readonly HashSet<string> ReaderExcludePath = new();

        private volatile uint _filesProcessed;

        public NodeReader(IConfigProvider configProvider, ICrcProvider? crcProvider, IChacheServise chacheServise)
        {
            _crcProvider = crcProvider;
            _chacheServise = chacheServise;

            var readerExcludePath = configProvider.GetValue("NodeReaderExcludePath");
            if (!string.IsNullOrEmpty(readerExcludePath))
            {
                readerExcludePath = Path.Combine(configProvider.GetAssetsPath(), readerExcludePath);
                using StreamReader sr = new(readerExcludePath);
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line) && line[0] != '#')
                    {

                        ReaderExcludePath.Add(line.Trim());

                    }
                }
            }
        }

        public uint GetFilesProcessed()
        {
            return _filesProcessed;
        }

        public async Task<FolderNode> CreateAsync(string fullPath, CancellationToken cancellationToken)
        {
            _filesProcessed = 0;
            fullPath = fullPath.ClearPath();
            var directoryInfo = new DirectoryInfo(fullPath);
            return await Task.Run(() => CreateFolderNode(directoryInfo, cancellationToken), cancellationToken).ConfigureAwait(false);
        }

        public FolderNode Create(string fullPath)
        {
            _filesProcessed = 0;
            fullPath = fullPath.ClearPath();
            var directoryInfo = new DirectoryInfo(fullPath);
            using CancellationTokenSource cancellationTokenSource = new();

            return CreateFolderNode(directoryInfo, cancellationTokenSource.Token);
        }

        private FolderNode CreateFolderNode(DirectoryInfo directoryInfo, CancellationToken cancellationToken)
        {
            var resultNode = new FolderNode(DateTime.UtcNow)
            {
                FullName = directoryInfo.FullName
            };

            FillFolder(resultNode, directoryInfo, cancellationToken);

            return resultNode;
        }

        private void FillFolder(FolderNode folder, DirectoryInfo directoryInfo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                foreach (var file in directoryInfo.GetFiles())
                {
                    var currFile = new FileNode
                    {
                        FullName = file.FullName,
                        Attributes = file.Attributes,
                        Length = file.Length,
                        CreationTime = file.CreationTimeUtc,
                        LastWriteTime = file.LastWriteTimeUtc,
                        Crc = GetCrc(file)
                    };

                    folder.Files.Add(currFile);
                    _filesProcessed++;
                }
            }
            catch (UnauthorizedAccessException)
            {

            }

            foreach (var directory in directoryInfo.GetDirectories())
            {
                try
                {
                    var currFolder = new FolderNode
                    {
                        FullName = directory.FullName,
                        CreationDate = directory.CreationTimeUtc
                    };
                    folder.Folders.Add(currFolder);

                    if (!ReaderExcludePath.Contains(currFolder.FullName))
                    {
                        FillFolder(currFolder, directory, cancellationToken);
                    }
                }
                catch (UnauthorizedAccessException)
                {

                }
            }
        }

        private uint GetCrc(FileInfo file)
        {
            var crc = _chacheServise.GetFromCache<uint>($"{CacheKeyPrefix}{file.FullName}");

            if (crc == 0 && _crcProvider != null)
            {
                crc = _crcProvider.GetFileChecksum(file.FullName);
                _ = _chacheServise.PutToCacheAsync($"{CacheKeyPrefix}{file.FullName}", crc, null);
            }

            return crc;
        }
    }
}
