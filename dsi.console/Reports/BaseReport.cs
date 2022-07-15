using dsi.common.Interfaces;
using dsi.common.Models;
using dsi.services;

namespace dsi.console.Reports
{
    public class BaseReport
    {
        protected const int RunFolderInternalSleepTime = 1000;

        protected readonly INodeReader _nodeReader;
        protected readonly INodeSerializer _nodeSerializer;
        protected readonly IReportProvider _reportProvider;
        protected readonly IConfigProvider _configProvider;
        protected readonly ILogger _logger;
        protected readonly CancellationTokenSource _cancellationTokenSource;

        public BaseReport(INodeReader nodeReader,
                INodeSerializer nodeSerializer,
                IReportProvider reportProvider,
                IConfigProvider configProvider,
                ILogger logger,
                CancellationTokenSource cancellationTokenSource)
        {
            _nodeReader = nodeReader ?? throw new ArgumentNullException(nameof(nodeReader));
            _nodeSerializer = nodeSerializer;
            _reportProvider = reportProvider;
            _configProvider = configProvider;
            _cancellationTokenSource = cancellationTokenSource;
            _logger = logger;
        }

        protected void SaveReport(List<string> report, string reportFileName)
        {
            var reportPath = Path.Combine(_configProvider.GetAssetsPath(), "Reports");
            if (!Directory.Exists(reportPath))
            {
                _ = Directory.CreateDirectory(reportPath);
            }

            reportFileName = reportFileName.Replace("%%DateTime%%", DateTime.Now.ToString("MMddHHmmss"));
            using StreamWriter sw = new(Path.Combine(reportPath, reportFileName));
            foreach (var line in report)
            {
                sw.WriteLine(line);
            }
            sw.Close();
            _logger.Info($"Формирование отчета {reportFileName} завершено.");
        }

        protected void WaitNodeReading(Task<FolderNode> folder, int predvFilesCount, string folderName)
        {
            while (!folder.IsCompleted && !folder.IsCanceled)
            {
                var filesProcessed = _nodeReader.GetFilesProcessed();
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
                        _logger.Info($"Обработка {folderName} была прервана, данные не полны!");
                        return;
                    }
                    Console.Clear();
                }
                Thread.Sleep(RunFolderInternalSleepTime);
            }

            Console.WriteLine();
            Console.WriteLine($"Чтение {folderName} завершено.");

            var actualFilesCount = folder.Result.GetTotalFilesCount();
            if (predvFilesCount != actualFilesCount)
            {
                AppSettingsProvider.SetValue($"TotalFilesCount_{folderName.ClearPath()}", actualFilesCount);
            }
        }
    }
}
