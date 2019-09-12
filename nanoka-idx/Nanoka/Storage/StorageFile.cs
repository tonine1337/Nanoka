using System.IO;

namespace Nanoka.Storage
{
    public class StorageFile
    {
        public string Name { get; set; }
        public Stream Stream { get; set; }
        public string MediaType { get; set; }
    }
}