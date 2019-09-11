using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum SnapshotType
    {
        [EnumMember(Value = "system")] System = 0,
        [EnumMember(Value = "user")] User = 1,
        [EnumMember(Value = "moderator")] Moderator = 2
    }
}