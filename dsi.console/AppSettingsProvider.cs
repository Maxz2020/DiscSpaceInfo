using System.Text.Json;

namespace dsi.console
{
    internal static class AppSettingsProvider
    {
        private static readonly string _fileName = Path.Combine(Environment.CurrentDirectory, "dci.console.appsettings.json");

        private static readonly Dictionary<string, object> _config = new();

        static AppSettingsProvider()
        {
            try
            {
                var configJson = File.ReadAllText(_fileName);
                _config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson) ?? new();
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void SetValue(string key, object value)
        {
            if (value == null)
            {
                return;
            }

            if (_config.ContainsKey(key))
            {
                _config[key] = value;
            }
            else
            {
                _config.Add(key, value);
            }

            var updatedConfigJson = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(_fileName, updatedConfigJson);
        }
        public static object? GetValue(string key)
        {
            if (_config.ContainsKey(key))
            {
                return _config[key];
            }
            else return null;
        }
    }
}
