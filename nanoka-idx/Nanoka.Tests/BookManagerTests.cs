using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nanoka.Database;
using Nanoka.Models;
using NUnit.Framework;

namespace Nanoka.Tests
{
    [TestFixture]
    public class BookManagerTests
    {
        [Test]
        public async Task CreateAsync()
        {
            using (var services = TestUtils.Services(
                c => c.Replace(ServiceDescriptor.Scoped<IUserClaims>(_ => new DummyUserClaimsProvider
                {
                    Id = ""
                }))))

            using (var scope = services.CreateScope())
            {
                await services.GetService<INanokaDatabase>().MigrateAsync();

                var books = scope.ServiceProvider.GetService<BookManager>();

                var bookModel = new BookBase
                {
                    Name = new[]
                    {
                        "my book",
                        "name 2"
                    },
                    Tags = new Dictionary<BookTag, string[]>
                    {
                        { BookTag.Artist, new[] { "artist", "artist2" } },
                        { BookTag.General, new[] { "tag" } }
                    }
                };

                var contentModel = new BookContentBase
                {
                    Language = LanguageType.English,
                    IsColor  = false,
                    Sources = new[]
                    {
                        new ExternalSource
                        {
                            Website    = "google.com",
                            Identifier = "book"
                        }
                    }
                };

                Book        book;
                BookContent content;

                using (var task = new UploadTask<object>(null))
                {
                    using (var memory = new MemoryStream())
                        await task.AddFileAsync("1.jpg", memory, "image/jpeg");

                    (book, content) = await books.CreateAsync(bookModel, contentModel, task);
                }

                Assert.That(book, Is.Not.Null);
                Assert.That(content, Is.Not.Null);

                var bookId    = book.Id;
                var contentId = content.Id;

                (book, content) = await books.GetContentAsync(bookId, contentId);

                Assert.That(book.Id, Is.EqualTo(bookId));
                Assert.That(content.Id, Is.EqualTo(contentId));

                Assert.That(book.Name, Is.Not.Null.Or.Empty);
                Assert.That(book.Name, Has.Exactly(2).Items);
                Assert.That(book.Name[0], Is.EqualTo("my book"));
                Assert.That(book.Tags, Is.Not.Null.Or.Empty);
                Assert.That(book.Tags, Has.Exactly(2).Items);
                Assert.That(book.Tags, Contains.Key(BookTag.Artist));
                Assert.That(book.Tags[BookTag.Artist], Has.Exactly(2).Items);
                Assert.That(book.Tags[BookTag.Artist][1], Is.EqualTo("artist 2"));

                Assert.That(content.PageCount, Is.EqualTo(1));
                Assert.That(content.Language, Is.EqualTo(LanguageType.English));
                Assert.That(content.IsColor, Is.False);
                Assert.That(content.Sources, Is.Not.Null.Or.Empty);
                Assert.That(content.Sources, Has.Exactly(1).Items);
                Assert.That(content.Sources[0].Website, Is.EqualTo("google.com"));
            }
        }
    }
}