using dsi.console.Reports;

namespace dsi.console
{
    internal class Duplicates
    {
        private readonly DuplicateReport _duplicateInfo;

        public Duplicates(DuplicateReport duplicateInfo)
        {
            _duplicateInfo = duplicateInfo;
        }

        public void Search(string paramStr)
        {
            var folders = paramStr.Trim().Split(';');

            _duplicateInfo.CreateReport(folders);
        }
    }
}
