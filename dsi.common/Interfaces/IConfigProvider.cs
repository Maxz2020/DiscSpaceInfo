namespace dsi.common.Interfaces
{
    /// <summary>
    /// Провайдер конфигурации
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Получить значение параметра
        /// </summary>
        /// <param name="key">Имя параметра</param>
        /// <returns></returns>
        string GetValue(string key);

        /// <summary>
        /// Возвращает путь к каталогу для настроек приложения
        /// </summary>
        /// <returns></returns>
        string GetAssetsPath();
    }
}
