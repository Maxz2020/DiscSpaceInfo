using dsi.common.Interfaces;
using dsi.services;
using System.IO;
using System.Threading;
using Xunit;

namespace dsi.tests
{
    public class NodeReaderTests : NodeTests
    {
        [Fact]
        public void NodeReaderCreateTest()
        {
            using var testDirectroy = new TestDirectroy();
            //configProvider.Setup(x => x.GetValue(It.IsAny<string>())).Returns(string.Empty);
            INodeReader nodeFactory = new NodeReader(ConfigProvider.Object, CrcProvider.Object, ChacheServise.Object);

            var folder = nodeFactory.Create(testDirectroy.WorkDir);

            Assert.NotNull(folder);
            Assert.NotNull(folder.Files);
            Assert.NotNull(folder.Folders);

            Assert.Single(folder.Files);
            Assert.Equal(TestDirectroy.NFolders, folder.Folders.Count);

            var name = Path.GetFileName(testDirectroy.WorkDir);
            Assert.Equal(name, folder.Name);
            Assert.Equal(testDirectroy.WorkDir, folder.FullName);

            var nFiles = folder.GetTotalFilesCount();
            Assert.Equal(TestDirectroy.NFiles, nFiles);

            var nFolders = folder.GetTotalFoldersCount();
            Assert.Equal(TestDirectroy.NFolders, nFolders);

            var totalLength = folder.GetTotalLength();
            Assert.Equal(TestDirectroy.TotalLength, totalLength);

            Assert.Equal(TestDirectroy.NFiles, folder.GetAllFiles().Count);
        }

        [Fact]
        public void NodeReaderCreateAsyncTest()
        {
            using var testDirectroy = new TestDirectroy();
            INodeReader nodeFactory = new NodeReader(ConfigProvider.Object, CrcProvider.Object, ChacheServise.Object);

            var folder = nodeFactory.CreateAsync(testDirectroy.WorkDir, CancellationTokenSource.Token).Result;

            Assert.NotNull(folder);
            Assert.NotNull(folder.Files);
            Assert.NotNull(folder.Folders);

            Assert.Single(folder.Files);
            Assert.Equal(TestDirectroy.NFolders, folder.Folders.Count);

            var name = Path.GetFileName(testDirectroy.WorkDir);
            Assert.Equal(name, folder.Name);
            Assert.Equal(testDirectroy.WorkDir, folder.FullName);

            var nFiles = folder.GetTotalFilesCount();
            Assert.Equal(TestDirectroy.NFiles, nFiles);

            var nFolders = folder.GetTotalFoldersCount();
            Assert.Equal(TestDirectroy.NFolders, nFolders);

            var totalLength = folder.GetTotalLength();
            Assert.Equal(TestDirectroy.TotalLength, totalLength);

            Assert.Equal(TestDirectroy.NFiles, folder.GetAllFiles().Count);
        }
    }
}