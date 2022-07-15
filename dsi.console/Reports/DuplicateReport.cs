using dsi.common.Interfaces;
using dsi.common.Models;
using dsi.services;

namespace dsi.console.Reports
{
    public class DuplicateReport : BaseReport
    {
        private const string ReportFileName = "_duplicateReport_%%DateTime%%.txt";
        private const string DataFolderName = "Data";

        private readonly IDuplicatesSearcher _duplicatesSearcher;

        public DuplicateReport(IDuplicatesSearcher duplicatesSearcher,
                INodeReader nodeReader,
                INodeSerializer nodeSerializer,
                IReportProvider reportProvider,
                IConfigProvider configProvider,
                ILogger logger,
                CancellationTokenSource cancellationTokenSource) : base(nodeReader, nodeSerializer, reportProvider, configProvider, logger, cancellationTokenSource)
        {
            _duplicatesSearcher = duplicatesSearcher;
        }

        public void CreateReport(string path)
        {
            CreateReport(new List<string> { path });
        }

        public void CreateReport(IEnumerable<string> pathList)
        {
            List<FolderNode> folders = new();

            foreach (string path in pathList)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            folders.Add(RunFolderInternal(path));
                        }
                        else
                        {
                            var filepath = path;
                            if (!File.Exists(filepath))
                            {
                                filepath = Path.Combine(_configProvider.GetAssetsPath(), DataFolderName, filepath);
                            }

                            if (File.Exists(filepath))
                            {
                                folders.Add(RunFileInternalAsync(filepath).Result);
                            }
                            else
                            {
                                _logger.Error($"Файлы: {path} или {filepath} не найдены!", true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, true);
                    }
                }
            }

            var duplicates = _duplicatesSearcher?.GetDuplicates(folders);

            var report = _reportProvider?.GetReport(duplicates);

            if (report != null)
            {
                SaveReport(report, ReportFileName);
            }
        }

        private async Task<FolderNode> RunFileInternalAsync(string fileName)
        {
            Console.CursorLeft = 0;
            Console.WriteLine($"Десериализация: {fileName}");

            FolderNode result;
            try
            {
                var compressedNode = await File.ReadAllBytesAsync(fileName, _cancellationTokenSource.Token);

                result = await _nodeSerializer.DeserializeAsync(compressedNode, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                Console.WriteLine(ex.Message);
                return new FolderNode();
            }

            Console.WriteLine($"Десериализация: {fileName} завершена.");
            return result;
        }

        private FolderNode RunFolderInternal(string folderName)
        {
            Console.CursorLeft = 0;
            folderName = folderName.ClearPath();

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
