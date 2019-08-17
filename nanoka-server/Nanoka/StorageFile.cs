using System.IO;

namespace Nanoka
{
    public class StorageFile
    {
        public string Name { get; set; }
        public Stream Stream { get; set; }
        public string ContentType { get; set; }
    }
}