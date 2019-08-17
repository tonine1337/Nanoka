using System;
using System.IO;

namespace Nanoka.Core
{
    public class TempDirectory : IDisposable
    {
        public string Path { get; }

        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            Directory.CreateDirectory(Path);
        }

        public void Dispose() => Directory.Delete(Path, true);
    }
}