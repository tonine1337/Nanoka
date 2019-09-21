using System;
using System.IO;

namespace Nanoka.Storage
{
    public class StorageFile : IDisposable
    {
        public string Name { get; set; }
        public Stream Stream { get; set; }
        public string MediaType { get; set; }

        public StorageFile() { }

        public StorageFile(string name, Stream stream, string mediaType)
        {
            Name      = name;
            Stream    = stream;
            MediaType = mediaType;
        }

        public void Dispose() => Stream?.Dispose();
    }
}