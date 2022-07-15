using MessagePack;

namespace dsi.common.Models
{
    /// <summary>
    /// Информация о файле в дереве каталогов
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class FileNode : Node
    {
        public FileAttributes Attributes { get; set; }

        public long Length { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        public uint Crc { get; set; }

        [IgnoreMember]
        public string? Extension => Path.GetExtension(Name);
    }
}
