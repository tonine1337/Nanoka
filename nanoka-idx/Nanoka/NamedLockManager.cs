using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka
{
    public class NamedLockManager : IDisposable
    {
        readonly object _lock = new object();

        // use plain Dictionary, not ConcurrentDictionary
        readonly Dictionary<object, Lock> _locks = new Dictionary<object, Lock>();

        // contains reusable semaphores to avoid recreating unnecessarily
        readonly Stack<SemaphoreSlim> _pool = new Stack<SemaphoreSlim>();

        // the maximum capacity of the semaphore pool
        const int _poolCapacity = 20;

        volatile bool _isDisposed;

        public NamedLockManager()
        {
            // preallocate semaphores
            /*for (var i = 0; i < _poolCapacity; i++)
                _pool.Push(new SemaphoreSlim(1));*/
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

            // wait for this semaphore
            await l.Semaphore.WaitAsync(cancellationToken);

            // we own this semaphore, assume caller calls Lock.Dispose
            return l;
        }

        sealed class Lock : IDisposable
        {
            readonly NamedLockManager _manager;
            readonly object _id;

            public readonly SemaphoreSlim Semaphore;

            public Lock(NamedLockManager manager, object id)
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
                        // return this semaphore to the pool if capacity not reached (to be reused later)
                        // if the manager is not disposed yet
                        if (_manager._pool.Count != _poolCapacity && !_manager._isDisposed)
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