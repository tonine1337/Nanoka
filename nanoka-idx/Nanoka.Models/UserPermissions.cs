using System;
using System.Runtime.Serialization;

namespace Nanoka.Models
{
    [Flags]
    public enum UserPermissions
    {
        /// <summary>
        /// User has no special permissions.
        /// </summary>
        [EnumMember(Value = "none")] None = 0,

        /// <summary>
        /// User is an administrator.
        /// This flag effectively enables every other flag.
        /// </summary>
        [EnumMember(Value = "admin")] Administrator = 1 << 0,

        /// <summary>
        /// User is a moderator.
        /// They can manage other user accounts.
        /// </summary>
        [EnumMember(Value = "mod")] Moderator = 1 << 1
    }
}