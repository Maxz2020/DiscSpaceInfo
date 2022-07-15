using dsi.common.Interfaces;
using dsi.services;
using System.Threading;
using Xunit;


namespace dsi.tests
{
    public class NodeComparerTests : NodeTests
    {
        [Fact]
        public void GetChangesAsyncTest()
        {
            using var testDirectroy = new TestDirectroy();
            INodeReader nodeFactory = new NodeReader(ConfigProvider.Object, CrcProvider.Object, ChacheServise.Object);

            var folder = nodeFactory.Create(testDirectroy.WorkDir);

            testDirectroy.DoChanges();

            var folderNew = nodeFactory.Create(testDirectroy.WorkDir);

            INodeComparer nodeComparer = new NodeComparer();
            var nodeChanges = nodeComparer.GetChangesAsync(folder, folderNew, CancellationTokenSource.Token).Result;

            var old = nodeChanges.FolderNodeChanges.GetTotalFilesCount(false, false);
            var oldDel = nodeChanges.FolderNodeChanges.GetTotalFilesCount(false, true);
            var oldNew = nodeChanges.FolderNodeChanges.GetTotalFilesCount(true, false);
            var oldNewDel = nodeChanges.FolderNodeChanges.GetTotalFilesCount(true, true);

            Assert.Equal(TestDirectroy.NnewFiles, oldNew - old);
            Assert.Equal(TestDirectroy.NdelFiles, oldDel - old);
            Assert.Equal(TestDirectroy.NFiles, oldDel);
            Assert.Equal(oldNewDel, oldNew + oldDel - old);
        }

        [Fact]
        public void GetChangesTest()
        {
            using var testDirectroy = new TestDirectroy();
            INodeReader nodeFactory = new NodeReader(ConfigProvider.Object, CrcProvider.Object, ChacheServise.Object);

            var folder = nodeFactory.Create(testDirectroy.WorkDir);

            testDirectroy.DoChanges();

            var folderNew = nodeFactory.Create(testDirectroy.WorkDir);

            INodeComparer nodeComparer = new NodeComparer();
            var nodeChanges = nodeComparer.GetChanges(folder, folderNew);

            var old = nodeChanges.FolderNodeChanges.GetTotalFilesCount(false, false);
            var oldDel = nodeChanges.FolderNodeChanges.GetTotalFilesCount(false, true);
            var oldNew = nodeChanges.FolderNodeChanges.GetTotalFilesCount(true, false);
            var oldNewDel = nodeChanges.FolderNodeChanges.GetTotalFilesCount(true, true);

            Assert.Equal(TestDirectroy.NnewFiles, oldNew - old);
            Assert.Equal(TestDirectroy.NdelFiles, oldDel - old);
            Assert.Equal(TestDirectroy.NFiles, oldDel);
            Assert.Equal(oldNewDel, oldNew + oldDel - old);
        }
    }
}
