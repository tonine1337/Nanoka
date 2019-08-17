using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka
{
    public abstract class UploadTask : IDisposable
    {
        readonly object _lock = new object();
        readonly List<TemporaryFile> _files = new List<TemporaryFile>();

        public Guid Id { get; }

        readonly DateTime _startTime = DateTime.UtcNow;

        DateTime _updateTime = DateTime.UtcNow;
        bool _active = true;

        public DateTime UpdateTime
        {
            get
            {
                lock (_lock)
                    return _updateTime;
            }
        }

        public UploadTask(Guid id)
        {
            Id = id;
        }

        public async Task AddFileAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var file = new TemporaryFile();

            try
            {
                // write to temporary file
                using (var dest = file.Open(FileMode.Open, FileAccess.Write))
                    await stream.CopyToAsync(dest, cancellationToken);

                // append to list
                lock (_lock)
                {
                    if (!_active)
                        return;

                    _files.Add(file);

                    _updateTime = DateTime.UtcNow;
                }
            }
            catch
            {
                file.Dispose();
                throw;
            }
        }

        public TemporaryFile[] GetFiles()
        {
            lock (_files)
                return _files.ToArray();
        }

        public void Dispose()
        {
            lock (_files)
            {
                foreach (var file in _files)
                    file.Dispose();

                _files.Clear();

                _active = false;
            }
        }

        public UploadState State
        {
            get
            {
                {
                    lock (_lock)
                    {
                        return new UploadState
                        {
                            Id        = Id,
                            Start     = _startTime,
                            Update    = _updateTime,
                            ItemCount = _files.Count,
                            IsActive  = _active
                        };
                    }
                }
            }
        }
    }
}