using MessagePack;

namespace dsi.common.Models
{
    /// <summary>
    /// Дополнительная информация для заголовка файла с сериализованными данными
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class HeaderData
    {
        public Dictionary<string, object> keyValuePairs = new();
    }
}
