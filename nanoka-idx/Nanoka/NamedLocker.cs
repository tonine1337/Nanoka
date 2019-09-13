using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Nanoka
{
    public interface ILocker : IDisposable
    {
        Task<IDisposable> EnterAsync(object id, CancellationToken cancellationToken = default);
    }

    public class NamedLocker : IDisposable
    {
        readonly Dictionary<Type, ILocker> _lockers = new Dictionary<Type, ILocker>();

        readonly object _sharedLock = new object();
        readonly Stack<SemaphoreSlim> _sharedPool = new Stack<SemaphoreSlim>();

        const int _poolCapacity = 200;

        public NamedLocker(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        readonly ILoggerFactory _loggerFactory;

        public ILocker Get<T>()
        {
            lock (_lockers)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(NamedLocker));

                if (!_lockers.TryGetValue(typeof(T), out var locker))
                    locker = _lockers[typeof(T)] = new NamedLockerInstance(_sharedLock, _sharedPool, _poolCapacity)
                    {
                        Logger = _loggerFactory.CreateLogger<T>()
                    };

                return locker;
            }
        }

        bool _isDisposed;

        public void Dispose()
        {
            lock (_lockers)
            {
                foreach (var locker in _lockers.Values)
                    locker.Dispose();

                _lockers.Clear();

                _isDisposed = true;
            }
        }
    }
}