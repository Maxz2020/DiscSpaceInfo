using dsi.common.Interfaces;
using dsi.common.Models;
using System.Text;

namespace dsi.services
{
    public class ReportProvider : IReportProvider
    {
        private readonly long _minFileLength;
        private readonly bool _duplicateReportPrintInfo = false;
        private readonly HashSet<string> DuplicateExcludePath = new();
        private readonly HashSet<string> DuplicateExcludeExtension = new();

        private readonly bool _changesReportPrintInfo = false;
        private readonly HashSet<string> ChangesExcludePath = new();
        private readonly HashSet<string> ChangesExcludeExtension = new();
        private long totalSizeChange = 0;

        public ReportProvider(IConfigProvider configProvider)
        {
            #region DuplicateReport
            var bufferSize = configProvider.GetValue("DuplicateReportMinFileLength");
            _ = long.TryParse(bufferSize, out _minFileLength);
            _minFileLength = _minFileLength > 0 ? _minFileLength : 1;

            var _printInfo = configProvider.GetValue("DuplicateReportPrintInfo");
            if (string.Equals(_printInfo, "true", StringComparison.Ordinal))
            {
                _duplicateReportPrintInfo = true;
            }

            var duplicateReportExcludePath = configProvider.GetValue("DuplicateReportExcludePath");
            if (!string.IsNullOrEmpty(duplicateReportExcludePath))
            {
                duplicateReportExcludePath = Path.Combine(configProvider.GetAssetsPath(), duplicateReportExcludePath);
                using StreamReader sr = new(duplicateReportExcludePath);
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line) && line[0] != '#')
                    {
                        if (line[0] == '.')
                        {
                            DuplicateExcludeExtension.Add(line.Trim());
                        }
                        else
                        {
                            DuplicateExcludePath.Add(line.Trim());
                        }
                    }
                }
            }
            #endregion

            #region ChangesReport
            _printInfo = configProvider.GetValue("ChangesReportPrintInfo");
            if (string.Equals(_printInfo, "true", StringComparison.Ordinal))
            {
                _changesReportPrintInfo = true;
            }

            var changesReportExcludePath = configProvider.GetValue("ChangesReportExcludePath");
            if (!string.IsNullOrEmpty(changesReportExcludePath))
            {
                changesReportExcludePath = Path.Combine(configProvider.GetAssetsPath(), changesReportExcludePath);
                using StreamReader sr = new(changesReportExcludePath);
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line) && line[0] != '#')
                    {
                        if (line[0] == '.')
                        {
                            ChangesExcludeExtension.Add(line.Trim());
                        }
                        else
                        {
                            ChangesExcludePath.Add(line.Trim());
                        }
                    }
                }
            }
            #endregion
        }

        public List<string> GetReport(IEnumerable<DuplicatesInfo>? duplicates)
        {
            List<string> report = new();

            if (duplicates == null)
            {
                return report;
            }

            StringBuilder stringBuilder = new();

            foreach (var files in duplicates)
            {
                if (files.Files[0].Length >= _minFileLength)
                {
                    var printHeader = false;

                    foreach (var file in files.Files)
                    {
                        if (!IsFileInExcludePath(file?.FullName, DuplicateExcludePath) && !IsFileInExcludeExtensions(file?.Extension, DuplicateExcludeExtension))
                        {
                            if (!printHeader)
                            {
                                _ = stringBuilder.AppendLine($"{ConvertLength(files.Files[0].Length)}");
                                printHeader = true;
                            }
                            _ = stringBuilder.AppendLine(file?.FullName);
                        }
                    }
                    if (printHeader)
                    {
                        report.Add(stringBuilder.ToString());
                    }
                    _ = stringBuilder.Clear();
                }
            }

            if (_duplicateReportPrintInfo)
            {
                _ = stringBuilder.Clear();
                _ = stringBuilder.AppendLine("------------ Report settings ------------");
                _ = stringBuilder.AppendLine($"DuplicateReportMinFileLength {ConvertLength(_minFileLength)} ({_minFileLength})");
                _ = stringBuilder.AppendLine($"Exclude file extensions: {string.Join(';', DuplicateExcludeExtension)}");
                _ = stringBuilder.AppendLine("Exclude paths:");
                foreach (var path in DuplicateExcludePath)
                {
                    stringBuilder.AppendLine(path);
                }
                report.Add(stringBuilder.ToString());
                _ = stringBuilder.Clear();
            }

            return report;
        }

        public List<string> GetReport(FolderNodeChangesContainer? changes)
        {
            List<string> report = new();

            if (changes == null)
            {
                return report;
            }

            StringBuilder stringBuilder = new();

            FillChangesRecursively(stringBuilder, changes.FolderNodeChanges);

            report.Add(stringBuilder.ToString());

            if (_changesReportPrintInfo)
            {
                _ = stringBuilder.Clear();
                _ = stringBuilder.AppendLine("------------ Report settings ------------");
                _ = stringBuilder.AppendLine($"Total: {ConvertLength(totalSizeChange, 8)}");
                _ = stringBuilder.AppendLine($"Satrt Date: {changes.StartChangesDate}, End Date: {changes.EndChangesDate}");
                _ = stringBuilder.AppendLine($"Satrt sourse: {changes.StartFileName} End sourse: {changes.EndFileName}");
                _ = stringBuilder.AppendLine($"Exclude file extensions: {string.Join(';', ChangesExcludeExtension)}");
                _ = stringBuilder.AppendLine("Exclude paths:");
                foreach (var path in ChangesExcludePath)
                {
                    stringBuilder.AppendLine(path);
                }
                report.Add(stringBuilder.ToString());
                _ = stringBuilder.Clear();
            }

            return report;
        }

        private void FillChangesRecursively(StringBuilder stringBuilder, FolderNodeChanges changes)
        {
            if (IsFileInExcludePath(changes.FullName, ChangesExcludePath))
            {
                return;
            }

            bool lineAdded = false;

            if (changes.IsNew)
            {
                totalSizeChange += changes.GetTotalLength();
                _ = stringBuilder.AppendLine($"++ {ConvertLength(changes.GetTotalLength(), 8)} {changes.FullName}");
                lineAdded = true;
            }
            if (changes.IsDeleted)
            {
                totalSizeChange -= changes.GetTotalLength();
                _ = stringBuilder.AppendLine($"-- {ConvertLength(changes.GetTotalLength(), 8)} {changes.FullName}");
                lineAdded = true;
            }

            foreach (var file in changes.Files)
            {
                if (IsFileInExcludeExtensions(file.Extension, ChangesExcludeExtension))
                {
                    continue;
                }

                if (file.IsFileChanged)
                {
                    var action = string.Empty;
                    var info = string.Empty;
                    if (file.IsDeleted)
                    {
                        action = "- ";
                    }
                    else if (file.IsNew)
                    {
                        action = "+ ";
                    }
                    else if (file.IsLengthChanged)
                    {
                        action = "~ ";
                        if (file.LengthOld > file.Length)
                        {
                            totalSizeChange = totalSizeChange - (file.LengthOld - file.Length);
                            info = $"(-{ConvertLength(file.LengthOld - file.Length)})";
                        }
                        else
                        {
                            totalSizeChange = totalSizeChange + (file.Length - file.LengthOld);
                            info = $"(+{ConvertLength(file.Length - file.LengthOld)})";
                        }
                    }
                    else if (file.IsCrcChanged && file.Crc != 0 && file.CrcOld != 0)
                    {
                        action = "~ ";
                        info = "(CRC)";
                    }

                    if (action != string.Empty)
                    {
                        _ = stringBuilder.AppendLine($"{action} {ConvertLength(file.Length, 8)} {file.FullName} {info}");
                        lineAdded = true;
                    }
                }
            }

            if (lineAdded)
            {
                _ = stringBuilder.AppendLine();
            }

            foreach (var folder in changes.Folders)
            {
                FillChangesRecursively(stringBuilder, folder);
            }
        }

        private static bool IsFileInExcludePath(string? fileFullName, HashSet<string> collection)
        {
            fileFullName += "\\";
            foreach (var excludeItem in collection)
            {
                if (fileFullName?.StartsWith(excludeItem) ?? false)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsFileInExcludeExtensions(string? fileExtension, HashSet<string> collection)
        {
            if (fileExtension == null)
            {
                return false;
            }

            return collection.Contains(fileExtension);
        }

        private static string ConvertLength(long length, byte resultStringLength = 0)
        {
            string? result;
            var k = 1;

            if (length < 0)
            {
                k = -1;
                length = Math.Abs(length);
            }

            if (length > 1000 * 1000)
            {
                result = $"{length * k / (1000 * 1000)} Мб.";
            }
            else if (length > 1000)
            {
                result = $"{length * k / 1000} Кб.";
            }
            else
            {
                result = $"{length * k} байт";
            }

            if (resultStringLength - result.Length > 0)
            {
                result += new string(' ', resultStringLength - result.Length);
            }

            return result;
        }
    }
}
