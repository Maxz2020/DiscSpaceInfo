using MessagePack;
using System.Diagnostics;


namespace dsi.common.Models
{
    /// <summary>
    /// Информация об изменениях в дереве каталогов
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true), DebuggerDisplay("FullName = {" + nameof(FullName) + "}")]
    public class FileNodeChanges : FileNode
    {
        public FileNodeChanges(FileNode fileNode, bool isDeleted = false)
        {
            FullName = fileNode.FullName;
            Attributes = fileNode.Attributes;
            Length = fileNode.Length;
            CreationTime = fileNode.CreationTime;
            LastWriteTime = fileNode.LastWriteTime;
            Crc = fileNode.Crc;
            IsDeleted = isDeleted;
        }

        public bool IsNew { get; set; }

        public bool IsDeleted { get; set; }

        [IgnoreMember]
        public bool IsLengthChanged => Length != LengthOld;

        [IgnoreMember]
        public bool IsCreationTimeChanged => CreationTime != CreationTimeOld;

        [IgnoreMember]
        public bool IsLastWriteTimeChanged => LastWriteTime != LastWriteTimeOld;

        [IgnoreMember]
        public bool IsCrcChanged => Crc != CrcOld;

        public long LengthOld { get; set; }

        public DateTime CreationTimeOld { get; set; }

        public DateTime LastWriteTimeOld { get; set; }

        public uint CrcOld { get; set; }

        [IgnoreMember]
        public bool IsFileChanged => IsNew || IsDeleted || IsLengthChanged || IsCrcChanged || IsLastWriteTimeChanged;

        public new string ToString()
        {
            return $"{FullName} created: {CreationTime:yyyy-MM-dd HH:mm}";
        }
    }
}
