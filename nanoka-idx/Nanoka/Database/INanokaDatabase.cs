using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Database
{
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
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