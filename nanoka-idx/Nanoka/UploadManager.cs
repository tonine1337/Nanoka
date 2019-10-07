using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nanoka
{
    public class UploadManager
    {
        readonly NanokaOptions _options;
        readonly UploadTaskCollection _tasks;
        readonly IUserClaims _claims;
        readonly ILoggerFactory _loggerFactory;

        public UploadManager(IOptions<NanokaOptions> options, UploadTaskCollection tasks, IUserClaims claims, ILoggerFactory loggerFactory)
        {
            _options       = options.Value;
            _tasks         = tasks;
            _claims        = claims;
            _loggerFactory = loggerFactory;
        }

        public UploadTask<T> CreateTask<T>(T data)
        {
            lock (_tasks)
            {
                if (_tasks.Values.Count(t => t.UploaderId == _claims.Id) == _options.UploadTaskLimitPerUser)
                    throw new InvalidOperationException($"May not create more than {_options.UploadTaskLimitPerUser} active upload tasks.");

                var task = new UploadTask<T>(data)
                {
                    UploaderId   = _claims.Id,
                    MaxFileCount = _options.MaxImageUploadCount,
                    Logger       = _loggerFactory.CreateLogger<UploadTask<T>>()
                };

                _tasks[task.Id] = task;

                return task;
            }
        }

        public UploadTask<T> GetTask<T>(string id)
        {
            lock (_tasks)
            {
                if (_tasks.TryGetValue(id, out var task) &&
                    task is UploadTask<T> genericTask &&
                    task.UploaderId == _claims.Id)
                    return genericTask;

                return null;
            }
        }

        /// <summary>
        /// Note: This does not dispose the task. The task should be disposed by the creator.
        /// </summary>
        public UploadTask<T> RemoveTask<T>(string id)
        {
            lock (_tasks)
            {
                var task = GetTask<T>(id);

                if (task != null)
                    _tasks.Remove(task.Id);

                return task;
            }
        }
    }

    public class UploadTaskCollection : Dictionary<string, UploadTask>, IDisposable
    {
        public void Dispose()
        {
            // nothing calls Dispose other than DI, so deadlock would not occur
            lock (this)
            {
                foreach (var task in Values)
                    task.Dispose();

                Clear();
            }
        }
    }
}