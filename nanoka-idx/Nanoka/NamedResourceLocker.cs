using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Nanoka
{
    public interface ILocker : IDisposable
    {
        Task<IDisposable> EnterAsync(object id, CancellationToken cancellationToken = default);
    }

    public class NamedResourceLocker : ILocker
    {
        readonly object _lock = new object();
        readonly ILogger<NamedResourceLocker> _logger;

        // use plain Dictionary, not ConcurrentDictionary
        readonly Dictionary<object, Lock> _locks = new Dictionary<object, Lock>();

        // contains reusable semaphores to avoid recreating every request
        readonly Stack<SemaphoreSlim> _pool = new Stack<SemaphoreSlim>();

        // the maximum capacity of the semaphore pool
        readonly int _poolCapacity;

        volatile bool _isDisposed;

        public NamedResourceLocker(ILogger<NamedResourceLocker> logger)
        {
            _logger = logger;

            // preallocate semaphores
            /*for (var i = 0; i < _poolCapacity; i++)
                _pool.Push(new SemaphoreSlim(1));*/

            // default pool capacity
            _poolCapacity = 100;
        }

        public async Task<IDisposable> EnterAsync(object id, CancellationToken cancellationToken = default)
        {
            Lock l;

            lock (_lock)
            {
                // get or create a new lock
                if (!_locks.TryGetValue(id, out l))
                    _locks[id] = l = new Lock(this, id);

                // increment reference count
                ++l.References;
            }

            using (var measure = new MeasureContext())
            {
                // wait for this semaphore
                await l.Semaphore.WaitAsync(cancellationToken);

                if (measure.Seconds >= 1)
                    _logger.LogWarning($"Took {measure} to obtain lock for resource '{id}'.");
            }

            // we own this semaphore; assume caller calls Lock.Dispose
            return l;
        }

        sealed class Lock : IDisposable
        {
            readonly NamedResourceLocker _manager;
            readonly object _id;

            public readonly SemaphoreSlim Semaphore;

            public Lock(NamedResourceLocker manager, object id)
            {
                _manager = manager;
                _id      = id;

                // try reusing semaphores
                if (!_manager._pool.TryPop(out Semaphore))
                    Semaphore = new SemaphoreSlim(1);
            }

            // reference counter always modified within lock
            public int References;

            // called Lock.Dispose
            public void Dispose()
            {
                lock (_manager._lock)
                {
                    // decrement reference count
                    if (--References == 0)
                    {
                        // we are the last reference to this lock
                        // return this semaphore to the pool if capacity not reached (to be reused later),
                        // if the manager is not disposed yet
                        if (_manager._pool.Count != _manager._poolCapacity && !_manager._isDisposed)
                        {
                            Semaphore.Release();

                            _manager._pool.Push(Semaphore);
                        }

                        // pool is full, so dispose semaphore and forget it
                        else
                        {
                            Semaphore.Dispose();
                        }

                        Trace.Assert(_manager._locks.Remove(_id), "someone hacked our semaphore dictionary");
                    }
                    else
                    {
                        // someone is still holding a reference to this lock; let them in
                        Semaphore.Release();
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed)
                    return;

                while (_pool.TryPop(out var semaphore))
                    semaphore.Dispose();

                _isDisposed = true;
            }
        }
    }
}