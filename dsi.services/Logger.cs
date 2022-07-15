using dsi.common.Interfaces;
using System.Text;

namespace dsi.services
{
    public class Logger : ILogger
    {
        public string ErrorFileName { get; }
        public string InfoFileName { get; }

        public Logger(IConfigProvider configProvider)
        {
            var errorFileName = configProvider.GetValue("ErrorLogFileName");
            ErrorFileName = Path.Combine(configProvider.GetAssetsPath(), errorFileName ?? "error.log");

            var infoFileName = configProvider.GetValue("InfoLogFileName");
            InfoFileName = Path.Combine(configProvider.GetAssetsPath(), infoFileName ?? "info.log");
        }

        public void Error(string message, bool console = false)
        {
            if (console)
            {
                Console.WriteLine(message);
            }

            using StreamWriter sw = new(ErrorFileName, true, Encoding.Default);
            try
            {
                sw.WriteLine($"{DateTime.Now}\n{message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Info(string message, bool console = false)
        {
            if (console)
            {
                Console.WriteLine(message);
            }

            using StreamWriter sw = new(InfoFileName, true, Encoding.Default);
            try
            {
                sw.WriteLine($"{DateTime.Now}\n{message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
