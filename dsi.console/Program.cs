using dsi.common.Interfaces;
using dsi.console.Reports;
using dsi.services;
using Microsoft.Extensions.DependencyInjection;

namespace dsi.console
{
    class Program
    {
        static void Main(string[] args)
        {
            CmdLineArgs cmdLineArgs;
            try
            {
                cmdLineArgs = new CmdLineArgs(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _ = Console.ReadKey();
                return;
            }

            var services = new ServiceCollection();
            _ = services.AddSingleton<CancellationTokenSource>();
            _ = services.AddSingleton<ILogger, Logger>();
            _ = services.AddSingleton<INodeSerializer, NodeSerializer>();
            _ = services.AddSingleton<IChacheServise, ChacheServise>();

            _ = services.AddSingleton<IConfigProvider>(serviceProvider => ActivatorUtilities.CreateInstance<ConfigProvider>(serviceProvider, cmdLineArgs.Options));

            _ = services.AddMemoryCache();

            _ = services.AddTransient<IReportProvider, ReportProvider>();
            _ = services.AddTransient<IDuplicatesSearcher, DuplicatesSearcher>();
            _ = services.AddTransient<ICrcProvider, CrcProvider>();
            _ = services.AddTransient<INodeReader, NodeReader>();
            _ = services.AddTransient<INodeComparer, NodeComparer>();
            _ = services.AddTransient<SerializedNodesHelper, SerializedNodesHelper>();
            _ = services.AddTransient<ChangesReport, ChangesReport>();
            _ = services.AddTransient<Changes, Changes>();
            _ = services.AddTransient<DuplicateReport, DuplicateReport>();
            _ = services.AddTransient<Duplicates, Duplicates>();
            _ = services.AddTransient<Serialize, Serialize>();
            _ = services.AddTransient<AutoChanges, AutoChanges>();

            var serviceProvider = services.BuildServiceProvider();

            switch (cmdLineArgs.Action)
            {
                case "-d":
                case "-D":
                    var duplicates = serviceProvider.GetService<Duplicates>();
                    duplicates?.Search(cmdLineArgs.Params[0]);
                    break;
                case "-c":
                case "-C":
                    var changes = serviceProvider.GetService<Changes>();
                    changes?.Search(cmdLineArgs.Params[0], cmdLineArgs.Params.Count > 2 ? cmdLineArgs.Params[1] : string.Empty);
                    break;
                case "-s":
                case "-S":
                    var serialize = serviceProvider.GetService<Serialize>();
                    serialize?.Run(cmdLineArgs.Params[0], cmdLineArgs.Params.Count > 2 ? cmdLineArgs.Params[1] : string.Empty);
                    break;
                case "-a":
                case "-A":
                    var autoChanges = serviceProvider.GetService<AutoChanges>();
                    autoChanges?.Run(cmdLineArgs.Params[0]);
                    break;
                default:
                    Console.WriteLine($"Ошибка: не поддерживаемое действие - {cmdLineArgs.Action}");
                    _ = Console.ReadKey();
                    break;
            }
        }
    }
}
