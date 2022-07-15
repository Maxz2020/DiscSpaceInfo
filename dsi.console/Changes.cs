using dsi.common.Interfaces;
using dsi.console.Reports;
using dsi.services;

namespace dsi.console
{
    internal class Changes
    {
        private const string DataFolderName = "Data";

        private readonly SerializedNodesHelper _serializedNodesHelper;
        private readonly ChangesReport _changesInfo;
        private readonly IConfigProvider _configProvider;
        private readonly ILogger _logger;

        public Changes(SerializedNodesHelper serializedNodesHelper, ChangesReport changesInfo, IConfigProvider configProvider, ILogger logger)
        {
            _serializedNodesHelper = serializedNodesHelper;
            _changesInfo = changesInfo;
            _configProvider = configProvider;
            _logger = logger;
        }

        public void Search(string paramStr1, string paramStr2)
        {

            paramStr1 = paramStr1.ClearPath();
            if (!Directory.Exists(paramStr1))
            {
                if (!File.Exists(paramStr1))
                {
                    var str = Path.Combine(_configProvider.GetAssetsPath(), DataFolderName, paramStr1);
                    if (File.Exists(str)) paramStr1 = str;
                }
            }

            paramStr2 = paramStr2.ClearPath();
            if (!Directory.Exists(paramStr2))
            {
                if (!File.Exists(paramStr2))
                {
                    var str = Path.Combine(_configProvider.GetAssetsPath(), DataFolderName, paramStr2);
                    if (File.Exists(str)) paramStr2 = str;
                }
            }

            if (!string.IsNullOrEmpty(paramStr2))
            {
                if (File.Exists(paramStr1) && File.Exists(paramStr2)) // Десериализация из файлов и сравнение между собой
                {
                    var oldNode = _serializedNodesHelper.DeserializeFromFileAsync(paramStr1);

                    var newNode = _serializedNodesHelper.DeserializeFromFileAsync(paramStr2);

                    _changesInfo.CreateReport(oldNode.Result, newNode.Result);
                }
                else if (Directory.Exists(paramStr1) && File.Exists(paramStr2)) //Чтение указанной папки и сравнение с указанным сериализованным файлом
                {
                    _changesInfo.CreateReport(paramStr1, paramStr2);
                }
                else
                {
                    var errMsg = $"Параметры {paramStr1} и {paramStr2} должны быть путями <\"Старый\" файл> <\"Новый\" файл> или <папка> <файл>";
                    _logger.Error(errMsg, true);
                }
            }
            else // Десериализация из файла и сравнение с соответствующей папкой
            {
                if (File.Exists(paramStr1))
                {
                    _changesInfo.CreateReport(paramStr1);
                }
                else
                {
                    var errMsg = $"Файл {paramStr1} не найден.";
                    _logger.Error(errMsg, true);
                }
            }
        }
    }
}
