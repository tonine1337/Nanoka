using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Nanoka
{
    public class UploadManager
    {
        internal static int NextId;

        readonly NanokaOptions _options;
        readonly UploadTaskCollection _tasks;
        readonly UserClaimSet _claims;

        public UploadManager(IOptions<NanokaOptions> options, UploadTaskCollection tasks, UserClaimSet claims)
        {
            _options = options.Value;
            _tasks   = tasks;
            _claims  = claims;
        }

        public UploadTask<T> CreateTask<T>(T data)
        {
            var task = new UploadTask<T>(data)
            {
                UploaderId   = _claims.Id,
                MaxFileCount = _options.MaxImageUploadCount
            };

            lock (_tasks)
                _tasks[task.Id] = task;

            return task;
        }

        public UploadTask<T> GetTask<T>(int id)
        {
            lock (_tasks)
            {
                if (_tasks.TryGetValue(id, out var task) &&
                    task is UploadTask<T> genericTask &&
                    task.UploaderId == _claims.Id)
                    return genericTask;

                throw Result.NotFound<UploadTask>(id).Exception;
            }
        }

        /// <summary>
        /// Note: This does not dispose the task. The task should be disposed by the creator.
        /// </summary>
        public UploadTask<T> RemoveTask<T>(int id)
        {
            lock (_tasks)
            {
                var task = GetTask<T>(id);

                _tasks.Remove(task.Id);

                return task;
            }
        }
    }

    public class UploadTaskCollection : Dictionary<int, UploadTask>, IDisposable
    {
        public void Dispose()
        {
            // nothing calls Dispose other than DI
            lock (this)
            {
                foreach (var task in Values)
                    task.Dispose();

                Clear();
            }
        }
    }
}