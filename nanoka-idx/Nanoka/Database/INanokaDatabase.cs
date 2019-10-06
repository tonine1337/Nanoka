using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Database
{
    public interface INanokaDatabase
        : IUserRepository,
          IBookRepository,
          IImageRepository,
          ISnapshotRepository,
          IVoteRepository,
          IDeleteFileRepository,
          IDisposable
    {
        Task MigrateAsync(CancellationToken cancellationToken = default);
        Task ResetAsync(CancellationToken cancellationToken = default);
    }
}