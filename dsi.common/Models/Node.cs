using MessagePack;
using System.Diagnostics;

namespace dsi.common.Models
{
    /// <summary>
    /// Базовый узел дерева каталогов
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true), DebuggerDisplay("FullName = {" + nameof(FullName) + "}")]
    public abstract class Node
    {
        public string FullName { get; set; } = string.Empty;

        [IgnoreMember]
        public string Name => Path.GetFileName(FullName);
    }
}
