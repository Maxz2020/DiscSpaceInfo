namespace dsi.common.Interfaces
{
    /// <summary>
    /// Логгер
    /// </summary>
    public interface ILogger
    {
        void Info(string message, bool console = false);

        void Error(string message, bool console = false);
    }
}
