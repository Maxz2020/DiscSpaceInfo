using dsi.common.Interfaces;
using dsi.common.Models;
using dsi.console.Reports;
using dsi.services;

namespace dsi.console
{
    internal class AutoChanges
    {
        private readonly SerializedNodesHelper _serializedNodesHelper;
        private readonly ILogger _logger;
        private readonly ChangesReport _changesInfo;

        public AutoChanges(SerializedNodesHelper serializedNodesHelper, ILogger logger, ChangesReport changesInfo)
        {
            _serializedNodesHelper = serializedNodesHelper;
            _logger = logger;
            _changesInfo = changesInfo;
        }

        public void Run(string paramStr)
        {
            DateTime yesterday = DateTime.UtcNow.AddDays(-1);
            yesterday = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day);

            var folders = paramStr.Trim().Split(';');

            for (int i = 0; i < folders.Length; i++)
            {
                folders[i] = folders[i].ClearPath();
                if (Directory.Exists(folders[i]))
                {
                    ProcessFolder(folders[i], yesterday);
                }
                else
                {
                    if (!string.IsNullOrEmpty(folders[i]))
                    {
                        _logger.Info($"Папка {folders[i]} не найдена.", true);
                    }
                }
            }
        }

        private void ProcessFolder(string folder, DateTime borderDay)
        {
            var filesInRegistry = _serializedNodesHelper.GetAllRegistryFiles();

            FolderNode? newNode = null;
            // "Старая" папка, если есть на указанный период
            var oldNode = _serializedNodesHelper.LoadFolderFromRegistryAsync(folder, borderDay).Result;

            // Имя файла с последней "новой" информацией о папке за указанный период
            var fileRegistryName = filesInRegistry.OrderByDescending(x => x.Item3)
                .FirstOrDefault(x => string.Equals(x.Item2, folder, StringComparison.CurrentCultureIgnoreCase) && x.Item3 > borderDay).Item1;

            // Если свежей информации нет - создаём и сохраняем в реестре
            if (string.IsNullOrEmpty(fileRegistryName))
            {
                newNode = _serializedNodesHelper.SaveFolderToRegistryAsync(folder).Result;
            }
            else if (oldNode != null) // Если есть "старая" информация, но при этом есть имя "свежего" файла - десериализуем "новые" данные
            {
                newNode = _serializedNodesHelper.DeserializeFromRegistryAsync(fileRegistryName).Result;
            }

            if (oldNode != null && newNode != null)
            {
                _changesInfo.CreateReport(oldNode, newNode);
            }
            else
            {
                _logger.Info($"Данные о папке {folder} за прошлий период для сравнения не найдены.", true);
            }
        }
    }
}
