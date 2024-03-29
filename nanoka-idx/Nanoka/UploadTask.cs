using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nanoka.Models;

namespace Nanoka
{
    public class UploadTask : IDisposable
    {
        public readonly string Id = Snowflake.New;

        protected readonly object Lock = new object();
        protected readonly List<FileInfo> Files = new List<FileInfo>();

        protected readonly DateTime StartTime = DateTime.UtcNow;

        internal string UploaderId;
        internal int MaxFileCount = int.MaxValue;

        protected struct FileInfo
        {
            public string Name;
            public TemporaryFile Handle;
            public string MediaType;
        }

        DateTime _updateTime = DateTime.UtcNow;

        public DateTime UpdateTime
        {
            get
            {
                lock (Lock)
                    return _updateTime;
            }
        }

        public ILogger Logger;

        public async Task AddFileAsync(string name, Stream stream, string mediaType, CancellationToken cancellationToken = default)
        {
            var file = new FileInfo
            {
                Name      = name,
                Handle    = new TemporaryFile(),
                MediaType = mediaType
            };

            try
            {
                using (var dest = file.Handle.Open(FileMode.Open, FileAccess.Write))
                    await stream.CopyToAsync(dest, cancellationToken);
            }
            catch
            {
                file.Handle.Dispose();
                throw;
            }

            lock (Lock)
            {
                if (_disposed)
                {
                    file.Handle.Dispose();
                    return;
                }

                if (Files.Count == MaxFileCount)
                {
                    file.Handle.Dispose();

                    throw new InvalidOperationException($"Maximum image upload limit reached for task {Id}.");
                }

                Files.Add(file);

                Logger?.LogInformation("Added file {0} '{1}' ({2}).", Files.Count, name, mediaType);

                _updateTime = DateTime.UtcNow;
            }
        }

        public IEnumerable<(string name, Stream stream, string mediaType)> EnumerateFiles()
        {
            lock (Lock)
                return Files.ToArray(f => (f.Name, f.Handle.Open(FileMode.Open, FileAccess.Read), f.MediaType));
        }

        public int FileCount
        {
            get
            {
                lock (Lock)
                    return Files.Count;
            }
        }

        bool _disposed;

        public void Dispose()
        {
            lock (Lock)
            {
                if (_disposed)
                    return;

                foreach (var file in Files)
                    file.Handle.Dispose();

                Files.Clear();

                _disposed = true;
            }
        }
    }

    public class UploadTask<T> : UploadTask
    {
        public readonly T Data;

        public UploadTask(T data)
        {
            Data = data;
        }

        UploadState GetState()
        {
            lock (Lock)
            {
                return new UploadState
                {
                    Id         = Id,
                    StartTime  = StartTime,
                    UploaderId = UploaderId,
                    FileCount  = FileCount
                };
            }
        }

        public static implicit operator UploadState(UploadTask<T> task) => task.GetState();
        public static implicit operator ActionResult<UploadState>(UploadTask<T> task) => task.GetState();
    }
}