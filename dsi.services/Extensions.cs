namespace dsi.services
{
    public static class Extensions
    {
        public static string ClearPath(this string path)
        {
            string? result = Path.TrimEndingDirectorySeparator(path.Trim());
            if (result.Length == 2)
            {
                result += "\\";
            }
            return result;
        }
    }
}
