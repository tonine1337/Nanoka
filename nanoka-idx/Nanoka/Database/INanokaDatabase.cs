using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Database
{
    // ReSharper disable PossibleInterfaceMemberAmbiguity
    public interface INanokaDatabase
        : IUserRepository,
          IBookRepository,
          IImageRepository,
          ISnapshotRepository,
          IVoteRepository,
          IDisposable
    {
        Task MigrateAsync(CancellationToken cancellationToken = default);
        Task ResetAsync(CancellationToken cancellationToken = default);
    }
}