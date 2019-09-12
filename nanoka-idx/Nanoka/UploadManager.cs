using System;
using System.Collections.Generic;

namespace Nanoka
{
    public class UploadManager
    {
        internal static int NextId;

        readonly UploadTaskCollection _tasks;
        readonly UserClaimSet _claims;

        public UploadManager(UploadTaskCollection tasks, UserClaimSet claims)
        {
            _tasks  = tasks;
            _claims = claims;
        }

        public UploadTask<T> CreateTask<T>(T data)
        {
            var task = new UploadTask<T>(data)
            {
                UploaderId = _claims.Id
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