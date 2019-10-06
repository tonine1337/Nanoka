using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface IBookRepository
    {
        Task<Book> GetAsync(string id, CancellationToken cancellationToken = default);

        Task UpdateAsync(Book book, CancellationToken cancellationToken = default);

        Task DeleteAsync(Book book, CancellationToken cancellationToken = default);

        Task<SearchResult<Book>> SearchAsync(BookQuery query, CancellationToken cancellationToken = default);
    }
}