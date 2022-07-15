using dsi.common.Interfaces;

namespace dsi.services
{
    public class ConfigProvider : IConfigProvider
    {
        public ConfigProvider(List<string> args) : this()
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    var value = arg.Split('=');
                    if (value.Length == 2)
                    {
                        var k = value[0].Remove(0, 2).Trim().ToLower();
                        var v = value[1].Trim();

                        if (Settings.ContainsKey(k))
                        {
                            Settings[k] = v;
                        }
                        else
                        {
                            Settings.Add(k, v);
                        }
                    }
                }
            }
        }

        public ConfigProvider()
        {
            if (!Directory.Exists(_assetsPath))
            {
                _ = Directory.CreateDirectory(_assetsPath);
                return;
            }

            try
            {
                using StreamReader sr = new(Path.Combine(_assetsPath, "config.txt"));

                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line) && line[0] != '#')
                    {
                        var value = line.Split('=');
                        if (value.Length == 2)
                        {
                            Settings.Add(value[0].Trim().ToLower(), value[1].Trim());
                        }
                    }
                }
                sr.Close();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Copy the contents of the \"Assets\" folder to your working directory.");
                Environment.Exit(1);
            }
        }

        public Dictionary<string, string> Settings { get; set; } = new();

        private readonly string _assetsPath = Environment.CurrentDirectory + "./Assets";

        public string GetValue(string key)
        {
            Settings.TryGetValue(key.ToLower(), out var loadModel);
            return loadModel ?? string.Empty;
        }

        public string GetAssetsPath()
        {
            return _assetsPath;
        }
    }
}
