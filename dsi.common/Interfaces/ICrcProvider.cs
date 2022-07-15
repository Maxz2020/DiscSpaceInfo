namespace dsi.common.Interfaces
{
    /// <summary>
    /// Расчет контрольной суммы файла
    /// </summary>
    public interface ICrcProvider
    {
        /// <summary>
        /// Получить контрольную сумму файла
        /// </summary>
        /// <param name="filename">Полное имя файла</param>
        /// <returns></returns>
        public uint GetFileChecksum(string filename);
    }
}
