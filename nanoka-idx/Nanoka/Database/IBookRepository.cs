using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface IBookRepository
    {
        Task<Book> GetBookAsync(string id, CancellationToken cancellationToken = default);

        Task UpdateBookAsync(Book book, CancellationToken cancellationToken = default);

        Task DeleteBookAsync(Book book, CancellationToken cancellationToken = default);

        Task<SearchResult<Book>> SearchBooksAsync(BookQuery query, CancellationToken cancellationToken = default);
    }
}