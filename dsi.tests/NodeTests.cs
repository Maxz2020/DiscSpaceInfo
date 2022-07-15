using dsi.common.Interfaces;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace dsi.tests
{
    public class NodeTests
    {
        protected CancellationTokenSource CancellationTokenSource { get; } = new();
        protected Mock<IConfigProvider> ConfigProvider { get; }
        protected Mock<ILogger> Logger { get; }

        protected Mock<ICrcProvider> CrcProvider { get; }

        protected Mock<IChacheServise> ChacheServise { get; }

        public NodeTests()
        {
            ConfigProvider = new Mock<IConfigProvider>();
            Logger = new Mock<ILogger>();
            CrcProvider = new Mock<ICrcProvider>();
            ChacheServise = new Mock<IChacheServise>();
        }

        protected sealed class TestDirectroy : IDisposable
        {
            public static int NFiles => 103;
            public static int NFolders => 3;
            public static int TotalLength => 124;
            public static int ManyFilesCount => 100;

            public static int NnewFiles => 104;
            public static int NdelFiles => 102;

            private bool _disposed = false;
            internal readonly string WorkDir;

            private const string FolderName1 = "Test1";
            private const string FolderName2 = "Test2";
            private const string FolderName3 = "Test3";
            private const string FolderName4ManyFiles = "TestMany";

            internal TestDirectroy()
            {
                var tempDir = Path.GetTempPath();

                if (Directory.Exists(WorkDir))
                {
                    Directory.Delete(WorkDir, true);
                }

                WorkDir = Directory.CreateDirectory(Path.Combine(tempDir, Path.GetRandomFileName())).FullName;

                var folder1 = Directory.CreateDirectory(Path.Combine(WorkDir, FolderName1)).FullName;
                var folder2 = Directory.CreateDirectory(Path.Combine(WorkDir, FolderName2)).FullName;
                var folder4 = Directory.CreateDirectory(Path.Combine(WorkDir, FolderName4ManyFiles)).FullName;

                using (var fileStream = File.Create(Path.Combine(WorkDir, "1.txt")))
                {
                    var info = new UTF8Encoding(true).GetBytes("Test 1 string.");
                    fileStream.Write(info, 0, info.Length);
                }

                using (var fileStream = File.Create(Path.Combine(folder1, "2.bin")))
                {
                    var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    fileStream.Write(data, 0, data.Length);
                }

                using (var fileStream = File.Create(Path.Combine(folder2, "empty")))
                {
                }

                for (byte i = 0; i < ManyFilesCount; i++)
                {
                    var fullFileName = Path.Combine(folder4, Path.GetRandomFileName());
                    using (var fileStream = File.Create(fullFileName))
                    {
                        var data = new byte[1];
                        data[0] = i;
                        fileStream.Write(data, 0, data.Length);
                    }
                }
            }

            public void DoChanges()
            {
                using (var fileStream = File.Create(Path.Combine(WorkDir, "11.txt")))
                {
                    var info = new UTF8Encoding(true).GetBytes("Test 11 string.");
                    fileStream.Write(info, 0, info.Length);
                }

                using (var fileStream = File.Create(Path.Combine(WorkDir, FolderName1, "22.txt")))
                {
                    var info = new UTF8Encoding(true).GetBytes("Test 22 string.");
                    fileStream.Write(info, 0, info.Length);
                }

                using (var fileStream = File.Create(Path.Combine(WorkDir, FolderName1, "222.txt")))
                {
                    var info = new UTF8Encoding(true).GetBytes("Test 222 string.");
                    fileStream.Write(info, 0, info.Length);
                }

                int j = 0;
                foreach (var file in Directory.GetFiles(Path.Combine(WorkDir, FolderName4ManyFiles)))
                {
                    if (j < ManyFilesCount / 2)
                    {
                        File.Delete(file);
                    }
                }

                for (int i = 0; i < 100; i++)
                {
                    using (var fileStream = File.Create(Path.Combine(WorkDir, FolderName4ManyFiles, Path.GetRandomFileName())))
                    {
                        var info = new UTF8Encoding(true).GetBytes(i.ToString());
                        fileStream.Write(info, 0, info.Length);
                    }
                }

                var folder3 = Directory.CreateDirectory(Path.Combine(WorkDir, FolderName3)).FullName;

                using (var fileStream = File.Create(Path.Combine(folder3, "3.dat")))
                {
                    var info = new UTF8Encoding(true).GetBytes("New Test 3 string.");
                    fileStream.Write(info, 0, info.Length);
                }

                Directory.Delete(Path.Combine(WorkDir, FolderName2), true);
                File.Delete(WorkDir + "\\1.txt");
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {

                    }

                    Directory.Delete(WorkDir, true);

                    _disposed = true;
                }
            }

            ~TestDirectroy()
            {
                Dispose(false);
            }

        }

    }
}
