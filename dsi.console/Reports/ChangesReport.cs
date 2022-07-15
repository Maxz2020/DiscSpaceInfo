using dsi.common.Interfaces;
using dsi.common.Models;

namespace dsi.console.Reports
{
    public class ChangesReport : BaseReport
    {
        private const string ReportFileName = "_changesReport_%%DateTime%%.txt";

        private readonly INodeComparer _nodeComparer;

        public ChangesReport(INodeComparer nodeComparer,
                INodeReader nodeReader,
                INodeSerializer nodeSerializer,
                IReportProvider reportProvider,
                IConfigProvider configProvider,
                ILogger logger,
                CancellationTokenSource cancellationTokenSource) : base(nodeReader, nodeSerializer, reportProvider, configProvider, logger, cancellationTokenSource)
        {
            _nodeComparer = nodeComparer;
        }

        /// <summary>
        /// Сравнение указанного сериализованного файла и соответствующей ему папки
        /// </summary>
        /// <param name="filePath">Путь к сериализованному файлу</param>
        public void CreateReport(string filePath)
        {
            try
            {
                var oldNode = RunFileInternalAsync(filePath).Result;

                var newNode = RunFolderInternal(oldNode.FullName);

                CreateReportInternal(oldNode, newNode, $"Прочитан фйал: {filePath}", $"Прочитан каталог: {newNode.FullName}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.Info(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, true);
            }
        }


        /// <summary>
        /// Сравнение указанной папки и указанного сериализованного файла
        /// </summary>
        /// <param name="filderpath"></param>
        /// <param name="filePath"></param>
        public void CreateReport(string folderPath, string filePath)
        {
            try
            {
                var oldNode = RunFileInternalAsync(filePath).Result;

                var newNode = RunFolderInternal(folderPath);

                CreateReportInternal(oldNode, newNode, $"Прочитан фйал: {filePath}", $"Прочитан каталог: {folderPath}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.Info(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, true);
            }
        }

        /// <summary>
        /// Сравнение полученного объекта и соответствующей ему папки
        /// </summary>
        /// <param name="oldNode">Объект для сравнения</param>
        public void CreateReport(FolderNode oldNode)
        {
            try
            {
                var newNode = RunFolderInternal(oldNode.FullName);

                CreateReportInternal(oldNode, newNode, string.Empty, $"Прочитан каталог: {newNode.FullName}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.Info(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, true);
            }
        }

        /// <summary>
        /// Сравнение 2х объектов
        /// </summary>
        /// <param name="oldNode">"Старый"</param>
        /// <param name="newNode">"Новый"</param>
        public void CreateReport(FolderNode oldNode, FolderNode newNode)
        {
            try
            {
                CreateReportInternal(oldNode, newNode, string.Empty, string.Empty);
            }
            catch (TaskCanceledException ex)
            {
                _logger.Info(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, true);
            }
        }

        private async void CreateReportInternal(FolderNode oldNode, FolderNode newNode, string oldNodeSourseName, string newNodeSourseName)
        {
            Console.CursorLeft = 0;
            Console.WriteLine($"Сравнение начато...");

            var changes = _nodeComparer.GetChangesAsync(oldNode, newNode, _cancellationTokenSource.Token);

            int predvFilesCount = -1;
            var obj = AppSettingsProvider.GetValue($"TotalFilesCount_{oldNode.FullName}");
            if (obj != null)
            {
                _ = int.TryParse(obj.ToString(), out predvFilesCount);
            }
            predvFilesCount *= 2;

            while (!changes.IsCompleted && !changes.IsCanceled)
            {
                var filesProcessed = _nodeComparer?.GetFilesProcessed() ?? 1;
                Console.CursorLeft = 0;
                Console.Write($"Файлов обработано: {filesProcessed}");
                if (predvFilesCount > 0 && filesProcessed > 0)
                {
                    double prc = (double)filesProcessed / predvFilesCount;

                    Console.Write($" - {Math.Round(prc * 100)}%");
                }

                Thread.Sleep(RunFolderInternalSleepTime);
            }

            Console.CursorLeft = 0;
            Console.Write($"Файлов обработано: {_nodeComparer?.GetFilesProcessed()} - 100%");
            Console.WriteLine();
            Console.WriteLine($"Сравнение завершено.");

            var nodeContainer = await changes;
            nodeContainer.StartFileName = oldNodeSourseName;
            nodeContainer.EndFileName = newNodeSourseName;

            var report = _reportProvider?.GetReport(changes.Result);

            if (report != null)
            {
                SaveReport(report, ReportFileName);
            }
        }

        private async Task<FolderNode> RunFileInternalAsync(string fileName)
        {
            Console.CursorLeft = 0;
            Console.Write($"Десериализация: {fileName}");

            FolderNode result;
            try
            {
                var compressedNode = await File.ReadAllBytesAsync(fileName, _cancellationTokenSource.Token);

                result = await _nodeSerializer.DeserializeAsync(compressedNode, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, true);
                return new FolderNode();
            }


            Console.CursorLeft = 0;
            Console.WriteLine($"Десериализация: {fileName} завершена.");
            return result;
        }

        private FolderNode RunFolderInternal(string? folderName)
        {
            if (!Directory.Exists(folderName))
            {
                throw new DirectoryNotFoundException(folderName);
            }

            Console.WriteLine($"Чтение: {folderName}");
            Console.CursorLeft = 0;

            var folder = _nodeReader.CreateAsync(folderName, _cancellationTokenSource.Token);

            int predvFilesCount = -1;
            var obj = AppSettingsProvider.GetValue($"TotalFilesCount_{folderName}");
            if (obj != null)
            {
                _ = int.TryParse(obj.ToString(), out predvFilesCount);
            }

            WaitNodeReading(folder, predvFilesCount, folderName);

            return folder.Result;
        }
    }
}
