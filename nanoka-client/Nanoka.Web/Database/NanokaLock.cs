using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Web.Database
{
    /// <summary>
    /// Centralized locking mechanism to limit access to resources.
    /// </summary>
    public static class NanokaLock
    {
        static readonly object _lock = new object();

        // use plain Dictionary, not ConcurrentDictionary
        static readonly Dictionary<object, Lock> _semaphores = new Dictionary<object, Lock>();

        // contains reusable semaphores to avoid recreating unnecessarily
        static readonly Stack<SemaphoreSlim> _pool = new Stack<SemaphoreSlim>();

        const int _poolCapacity = 100;

        static NanokaLock()
        {
            // preallocate semaphores
            for (var i = 0; i < _poolCapacity; i++)
                _pool.Push(new SemaphoreSlim(1));
        }

        public static async Task<IDisposable> EnterAsync(object id, CancellationToken cancellationToken = default)
        {
            Lock l;

            lock (_lock)
            {
                // get or create a new lock
                if (!_semaphores.TryGetValue(id, out l))
                    _semaphores[id] = l = new Lock(id);

                // increment reference count
                ++l.References;
            }

            // wait for this semaphore
            await l.Semaphore.WaitAsync(cancellationToken);

            // we own this semaphore, assume caller calls Lock.Dispose
            return l;
        }

        sealed class Lock : IDisposable
        {
            readonly object _id;

            public readonly SemaphoreSlim Semaphore;

            public Lock(object id)
            {
                _id = id;

                // try reusing semaphores
                if (!_pool.TryPop(out Semaphore))
                    Semaphore = new SemaphoreSlim(1);
            }

            public int References;

            // caller called Lock.Dispose
            public void Dispose()
            {
                lock (_lock)
                {
                    // decrement reference count
                    if (--References == 0)
                    {
                        // we are the last reference to this lock
                        // return this semaphore to the pool if capacity not reached (to be reused later)
                        if (_pool.Count != _poolCapacity)
                            _pool.Push(Semaphore);

                        // pool is full, so dispose
                        else
                            Semaphore.Dispose();

                        Trace.Assert(_semaphores.Remove(_id), "someone hacked our semaphore dictionary");
                    }
                    else
                    {
                        // someone is still holding a reference to this lock; let them in
                        Semaphore.Release();
                    }
                }
            }
        }
    }
}