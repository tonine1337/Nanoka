using System;
using System.IO;

namespace Nanoka.Storage
{
    public class StorageFile : IDisposable
    {
        public string Name { get; set; }
        public Stream Stream { get; set; }
        public string MediaType { get; set; }

        public void Dispose() => Stream.Dispose();
    }
}