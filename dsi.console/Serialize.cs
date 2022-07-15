using dsi.common.Interfaces;
using dsi.services;

namespace dsi.console
{
    public class Serialize
    {
        private readonly SerializedNodesHelper _serializedNodesHelper;
        private readonly ILogger _logger;

        public Serialize(SerializedNodesHelper serializedNodesHelper, ILogger logger)
        {
            _serializedNodesHelper = serializedNodesHelper;
            _logger = logger;
        }

        public void Run(string paramStr1, string paramStr2)
        {
            paramStr1 = paramStr1.ClearPath();
            paramStr2 = paramStr2.ClearPath();

            if (!string.IsNullOrEmpty(paramStr2) && Directory.Exists(paramStr1))
            {
                if (File.Exists(paramStr2))
                {
                    Console.WriteLine($"Файл {paramStr2} уже существует. Перезаписать? (Y/N)");
                    var key = Console.ReadLine();
                    if (key != "y" && key != "Y")
                    {
                        Console.WriteLine($"Операция прервана.");
                        return;
                    }
                }

                _serializedNodesHelper.SaveFolderInfoToFileAsync(paramStr1, paramStr2).Wait();
                Console.WriteLine($"Файл {paramStr2} сохранён.");
            }
            else if (Directory.Exists(paramStr1))
            {
                _serializedNodesHelper.SaveFolderToRegistryAsync(paramStr1).Wait();
            }
            else
            {
                var errMsg = $"Параметры {paramStr1} и {paramStr2} должны быть путями <папка> <файл>.";
                _logger.Error(errMsg);
                Console.WriteLine(errMsg);
            }
        }
    }
}
