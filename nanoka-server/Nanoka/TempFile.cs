using System;
using System.IO;

namespace Nanoka
{
    public class TempFile : IDisposable
    {
        public string Path { get; }

        public TempFile()
        {
            Path = System.IO.Path.GetTempFileName();
        }

        public Stream Open(FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite)
            => File.Open(Path, mode, access);

        public void Dispose() => File.Delete(Path);
    }
}