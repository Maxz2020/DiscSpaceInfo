using dsi.common.Interfaces;
using dsi.services;
using System.Threading;
using Xunit;

namespace dsi.tests
{
    public class NodeSerializerTests : NodeTests
    {
        [Fact]
        public void SerializeDeserializeTest()
        {
            using var testDirectroy = new TestDirectroy();
            INodeReader nodeFactory = new NodeReader(ConfigProvider.Object, CrcProvider.Object, ChacheServise.Object);
            var folder = nodeFactory.CreateAsync(testDirectroy.WorkDir, CancellationTokenSource.Token).Result;

            INodeSerializer nodeSerializer = new NodeSerializer(Logger.Object);
            Assert.NotNull(nodeSerializer);

            #region SerializeAsync
            var serializedbBytes = nodeSerializer.SerializeAsync(folder, CancellationTokenSource.Token).Result;
            Assert.NotNull(serializedbBytes);
            #endregion

            #region DeserializeHeaderAdditionalDataAsync
            var deserializedHeader = nodeSerializer.DeserializeHeaderAdditionalDataAsync(serializedbBytes, CancellationTokenSource.Token).Result;
            #endregion         

            #region DeserializeAsync
            var deserializedNode = nodeSerializer.DeserializeAsync(serializedbBytes, CancellationTokenSource.Token).Result;
            Assert.NotNull(deserializedNode);

            Assert.Equal(folder.Files.Count, deserializedNode.Files.Count);
            Assert.Equal(folder.Folders.Count, deserializedNode.Folders.Count);

            Assert.Equal(folder.Name, deserializedNode.Name);
            Assert.Equal(folder.FullName, deserializedNode.FullName);

            var totalLength = deserializedNode.GetTotalLength();
            Assert.Equal(folder.GetTotalLength(), totalLength);
            #endregion

            #region Deserialize
            deserializedNode = nodeSerializer.Deserialize(serializedbBytes);
            Assert.NotNull(deserializedNode);

            Assert.Equal(folder.Files.Count, deserializedNode.Files.Count);
            Assert.Equal(folder.Folders.Count, deserializedNode.Folders.Count);

            Assert.Equal(folder.Name, deserializedNode.Name);
            Assert.Equal(folder.FullName, deserializedNode.FullName);

            totalLength = deserializedNode.GetTotalLength();
            Assert.Equal(folder.GetTotalLength(), totalLength);
            #endregion
        }
    }
}
