using dsi.common.Interfaces;
using dsi.common.Models;
using dsi.services;
using System.Text;

namespace dsi.console
{
    public class SerializedNodesHelper
    {
        protected const int RunFolderInternalSleepTime = 1000;
        private const string NodeFileName = "folderNode_%%DateTime%%.dsi";
        private const string InfoFileName = "datainfo.txt";
        private const string CacheKeyPrefix = "Registry_";
        private const string NodeFileExtension = "*.dsi";
        private const string DataFolderName = "Data";

        private const string NodeFullNameKey = "NodeFullName";
        private const string NodeSaveDateKey = "NodeSaveDate";

        private readonly string WorkPath;

        private readonly INodeSerializer _nodeSerializer;
        private readonly INodeReader _nodeReader;
        private readonly IConfigProvider _configProvider;
        private readonly ILogger _logger;
        private readonly IChacheServise _chacheServise;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public SerializedNodesHelper(INodeSerializer nodeSerializer,
                INodeReader nodeReader,
                IConfigProvider configProvider,
                ILogger logger,
                IChacheServise chacheServise,
                CancellationTokenSource cancellationTokenSource)
        {
            _nodeSerializer = nodeSerializer;
            _nodeReader = nodeReader;
            _configProvider = configProvider;
            _logger = logger;
            _chacheServise = chacheServise;
            _cancellationTokenSource = cancellationTokenSource;

            WorkPath = Path.Combine(_configProvider.GetAssetsPath(), DataFolderName);
            if (!Directory.Exists(WorkPath))
            {
                _ = Directory.CreateDirectory(WorkPath);
            }
        }

        /// <summary>
        /// Прочитать, сериализовать и сохранить каталог в файл
        /// </summary>
        /// <param name="folderName">Путь к сохраняемому каталогу</param>
        /// /// <param name="fileName">Путь к сохраняемому файлу</param>
        public async Task SaveFolderInfoToFileAsync(string folderName, string fileName)
        {
            var node = RunFolderInternal(folderName);
            await SerializeToFileAsync(node, Path.Combine(WorkPath, fileName));
        }

        /// <summary>
        /// Прочитать, сериализовать и сохранить каталог в хранилище
        /// </summary>
        /// <param name="folderName">Путь к сохраняемому каталогу</param>
        public async Task<FolderNode> SaveFolderToRegistryAsync(string folderName)
        {
            var node = RunFolderInternal(folderName);

            var reportFileName = NodeFileName.Replace("%%DateTime%%", DateTime.Now.ToString("MMddHHmmss"));

            await SerializeToFileAsync(node, Path.Combine(WorkPath, reportFileName));

            _ = _chacheServise.PutToCacheAsync($"{CacheKeyPrefix}{reportFileName}", new FolderNodeInfo(node.FullName, node.CreationDate), null);

            using StreamWriter sw = new(Path.Combine(WorkPath, InfoFileName), true, Encoding.Default);

            try
            {
                sw.WriteLine($"{DateTime.Now:yy-MM-dd HH:mm:ss} {reportFileName} \"{folderName}\"");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }

            return node;
        }

        /// <summary>
        /// Получить информацию о всех файлах в хранилище
        /// </summary>
        /// <returns></returns>
        public List<(string, string, DateTime)> GetAllRegistryFiles()
        {
            List<(string, string, DateTime)> result = new();

            foreach (var FullFileName in Directory.EnumerateFiles(WorkPath, NodeFileExtension))
            {
                var fileName = Path.GetFileName(FullFileName);

                var folderNodeInfo = _chacheServise.GetFromCache<FolderNodeInfo>($"{CacheKeyPrefix}{fileName}");
                if (folderNodeInfo == null)
                {
                    folderNodeInfo = GetFolderNodeInfoFromFileAsync(FullFileName).Result;
                    _ = _chacheServise.PutToCacheAsync($"{CacheKeyPrefix}{fileName}", folderNodeInfo, null);
                }
                result.Add((fileName, folderNodeInfo.FullName, folderNodeInfo.CreationDate));

            }

            return result;
        }

        /// <summary>
        /// Найти и десериализовать из хранилища самый последний по дате каталог
        /// </summary>
        /// <param name="folderName">Путь к искомому каталогу</param>
        /// <param name="dateBeforeUtc">Дата до которой должна быть информация</param>
        /// <returns></returns>
        public async Task<FolderNode?> LoadFolderFromRegistryAsync(string folderName, DateTime dateBeforeUtc)
        {
            DateTime maxDateTime = DateTime.MinValue;
            FolderNode? actualFolderNode = null;
            string actualFileName = string.Empty;

            foreach (var FullFileName in Directory.EnumerateFiles(WorkPath, NodeFileExtension))
            {
                var fileName = Path.GetFileName(FullFileName);
                var folderNodeInfo = _chacheServise.GetFromCache<FolderNodeInfo>($"{CacheKeyPrefix}{fileName}");

                if (folderNodeInfo == null)
                {
                    folderNodeInfo = GetFolderNodeInfoFromFileAsync(FullFileName).Result;
                    _ = _chacheServise.PutToCacheAsync($"{CacheKeyPrefix}{fileName}", folderNodeInfo, null);
                }

                if (folderNodeInfo.FullName == folderName)
                {

                    if (maxDateTime < folderNodeInfo.CreationDate && folderNodeInfo.CreationDate < dateBeforeUtc)
                    {
                        maxDateTime = folderNodeInfo.CreationDate;
                        actualFileName = FullFileName;
                    }
                }
            }

            if (!string.IsNullOrEmpty(actualFileName))
            {
                actualFolderNode = await DeserializeFromFileAsync(actualFileName);
            }

            if (actualFolderNode == null)
            {
                _logger.Info($"Информации о состоянии {folderName} до даты {dateBeforeUtc} не найдено.");
            }
            else
            {
                _logger.Info($"Информации о состоянии {folderName} взята из файла {actualFileName}");
            }

            return actualFolderNode;
        }

        /// <summary>
        /// Сериализация прочитанного каталога и сохранение в файл
        /// </summary>
        /// <param name="node">Каталог для сериализации</param>
        /// <param name="filename">Имя файла для сохранения</param>
        /// <returns></returns>
        public async Task SerializeToFileAsync(FolderNode node, string filename)
        {
            try
            {
                var data = await _nodeSerializer.SerializeAsync(node, _cancellationTokenSource.Token);

                await File.WriteAllBytesAsync(filename, data, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Десериализация каталога из указанного файла в реестре
        /// </summary>
        /// <param name="filename">Имя файла в реестре</param>
        /// <returns></returns>
        public async Task<FolderNode> DeserializeFromRegistryAsync(string filename)
        {
            filename = Path.Combine(WorkPath, filename);
            return await DeserializeFromFileAsync(filename);
        }

        /// <summary>
        /// Десериализация каталога из указанного файла
        /// </summary>
        /// <param name="filename">Путь к файлу для десериализации</param>
        /// <returns></returns>
        public async Task<FolderNode> DeserializeFromFileAsync(string filename)
        {
            byte[] compressedNode;
            try
            {
                compressedNode = await File.ReadAllBytesAsync(filename, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }

            return await _nodeSerializer.DeserializeAsync(compressedNode, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Получение дополнительной информации о данных содержащихся в классе FolderNode
        /// </summary>
        /// <param name="filename">Путь к файлу для десериализации</param>
        /// <returns></returns>
        public async Task<FolderNodeInfo> GetFolderNodeInfoFromFileAsync(string filename)
        {
            byte[] compressedNode;
            try
            {
                compressedNode = await File.ReadAllBytesAsync(filename, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }

            var headerData = await _nodeSerializer.DeserializeHeaderAdditionalDataAsync(compressedNode, _cancellationTokenSource.Token);

            string nodeFullName = string.Empty;
            DateTime? nodeSaveDate = null;
            try
            {
                nodeFullName = (string)headerData.keyValuePairs.FirstOrDefault(x => x.Key == NodeFullNameKey).Value;
                nodeSaveDate = (DateTime?)headerData.keyValuePairs.FirstOrDefault(x => x.Key == NodeSaveDateKey).Value;
            }
            catch (InvalidCastException)
            {

            }

            if (!string.IsNullOrWhiteSpace(nodeFullName) && nodeSaveDate != null)
            {
                return new FolderNodeInfo(nodeFullName, (DateTime)nodeSaveDate);
            }

            var folderNode = await _nodeSerializer.DeserializeAsync(compressedNode, _cancellationTokenSource.Token);

            return new FolderNodeInfo(folderNode.FullName, folderNode.CreationDate);
        }

        private FolderNode RunFolderInternal(string? folderName)
        {
            if (!Directory.Exists(folderName))
            {
                throw new DirectoryNotFoundException(folderName);
            }

            Console.WriteLine($"Чтение: {folderName}");
            Console.CursorLeft = 0;

            var folder = _nodeReader?.CreateAsync(folderName, _cancellationTokenSource.Token);
            if (folder == null)
            {
                _logger.Error($"При чтении {folderName} произошла ошибка.");
                return new FolderNode();
            }

            int predvFilesCount = -1;
            var obj = AppSettingsProvider.GetValue($"TotalFilesCount_{folderName}");
            if (obj != null)
            {
                _ = int.TryParse(obj.ToString(), out predvFilesCount);
            }

            WaitNodeReading(folder, predvFilesCount, folderName);

            return folder.Result;
        }

        protected void WaitNodeReading(Task<FolderNode> folder, int predvFilesCount, string folderName)
        {
            while (!folder.IsCompleted && !folder.IsCanceled)
            {
                var filesProcessed = _nodeReader?.GetFilesProcessed() ?? 1;
                Console.CursorLeft = 0;
                Console.Write($"Файлов обработано: {filesProcessed}");
                if (predvFilesCount > 0 && filesProcessed > 0)
                {
                    double prc = (double)filesProcessed / predvFilesCount;

                    Console.Write($" - {Math.Round(prc * 100)}%");
                }

                if (Console.KeyAvailable)
                {
                    Console.WriteLine();
                    Console.Write($"Остановить работу? (Y/N)");
                    _ = Console.ReadKey();
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                    {
                        _cancellationTokenSource.Cancel();
                        _logger.Info($"Обработка {folderName} была прервана, данные по изменениям не полны!");
                        return;
                    }
                    Console.Clear();
                }
                Thread.Sleep(RunFolderInternalSleepTime);
            }

            var actualFilesCount = folder.Result.GetTotalFilesCount();
            if (predvFilesCount != actualFilesCount)
            {
                AppSettingsProvider.SetValue($"TotalFilesCount_{folderName.ClearPath()}", actualFilesCount);
            }

            Console.CursorLeft = 0;
            Console.Write($"Файлов обработано: {actualFilesCount} - 100%");
            Console.WriteLine();
            Console.WriteLine($"Чтение {folderName} завершено.");           
        }
    }
}
