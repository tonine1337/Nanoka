using System;
using System.IO;

namespace Nanoka
{
    public class TemporaryFile : IDisposable
    {
        public string Path { get; }

        public TemporaryFile()
        {
            Path = System.IO.Path.GetTempFileName();
        }

        public Stream Open(FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite)
            => File.Open(Path, mode, access);

        public void Dispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch
            {
                // ignored
            }
        }
    }
}