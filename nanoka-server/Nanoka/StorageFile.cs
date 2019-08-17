using System.IO;

namespace Nanoka
{
    public class StorageFile
    {
        public Stream Stream { get; set; }
        public string ContentType { get; set; }
    }
}