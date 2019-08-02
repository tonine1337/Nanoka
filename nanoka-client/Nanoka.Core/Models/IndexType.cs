using System;

namespace Nanoka.Core.Models
{
    [Flags]
    public enum IndexType
    {
        Doujinshi = 1 << 0,
        Booru = 1 << 1
    }
}