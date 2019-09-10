using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum UploadStatus
    {
        [EnumMember(Value = "running")] Running = 0,
        [EnumMember(Value = "successful")] Successful = 1,
        [EnumMember(Value = "canceled")] Canceled = 2,
        [EnumMember(Value = "timed_out")] TimedOut = 3,
        [EnumMember(Value = "failed")] Failed = 4
    }
}