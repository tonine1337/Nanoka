using System;

namespace Nanoka.Web
{
    public static class GuidUtility
    {
        public static string ToShortString(this Guid guid)
            => Convert.ToBase64String(guid.ToByteArray())
                      .Substring(0, 22)
                      .Replace("/", "_")
                      .Replace("+", "-");

        public static Guid ToGuid(this string str)
            => new Guid(Convert.FromBase64String(str.Replace("_", "/").Replace("-", "+") + "=="));
    }
}