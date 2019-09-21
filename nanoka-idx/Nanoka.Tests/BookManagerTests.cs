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
                c => c.Replace(ServiceDescriptor.Scoped<IUserClaims>(_ => new DummyUserClaimsProvider()))))

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
                        { BookTag.Artist, new[] { "artist", "artist 2" } },
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

                using (var task = new UploadTask())
                {
                    using (var memory = new MemoryStream())
                        await task.AddFileAsync("1.jpg", memory, "image/jpeg");

                    book = await books.CreateAsync(bookModel, contentModel, task);
                }

                Assert.That(book, Is.Not.Null);
                Assert.That(book.Contents, Is.Not.Null.Or.Empty);
                Assert.That(book.Contents, Has.Exactly(1).Items);

                var bookId    = book.Id;
                var contentId = book.Contents[0].Id;

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
                Assert.That(book.Contents, Has.Exactly(1).Items);
                Assert.That(book.Contents, Contains.Item(content));

                Assert.That(content.PageCount, Is.EqualTo(1));
                Assert.That(content.Language, Is.EqualTo(LanguageType.English));
                Assert.That(content.IsColor, Is.False);
                Assert.That(content.Sources, Is.Not.Null.Or.Empty);
                Assert.That(content.Sources, Has.Exactly(1).Items);
                Assert.That(content.Sources[0].Website, Is.EqualTo("google.com"));

                Book        book2;
                BookContent content2;

                var contentModel2 = new BookContentBase
                {
                    Language = LanguageType.ChineseSimplified,
                    IsColor  = true
                };

                using (var task = new UploadTask())
                {
                    using (var memory = new MemoryStream())
                    {
                        await task.AddFileAsync("1.jpg", memory, "image/jpeg");
                        await task.AddFileAsync("2.jpg", memory, "image/jpeg");
                        await task.AddFileAsync("3.png", memory, "image/png");
                    }

                    (book2, content2) = await books.AddContentAsync(book.Id, contentModel2, task);
                }

                Assert.That(book2, Is.Not.Null);
                Assert.That(book2.Id, Is.EqualTo(book.Id));
                Assert.That(book2.Name, Is.EqualTo(book.Name));
                Assert.That(book2.Contents.Length, Is.Not.EqualTo(book.Contents.Length));
                Assert.That(book2.Contents, Contains.Item(content2));

                Assert.That(book2.Contents[0].PageCount, Is.EqualTo(1));
                Assert.That(book2.Contents[1].PageCount, Is.EqualTo(3));
                Assert.That(book2.Contents[1].IsColor, Is.True);
            }
        }

        [Test]
        public async Task TestHistoryAsync()
        {
            using (var services = TestUtils.Services(
                c => c.Replace(ServiceDescriptor.Scoped<IUserClaims>(_ => new DummyUserClaimsProvider
                {
                    Id = "userId",
                    QueryParams = new Dictionary<string, string>
                    {
                        { "reverse", "true" }
                    }
                }))))

            using (var scope = services.CreateScope())
            {
                await services.GetService<INanokaDatabase>().MigrateAsync();

                var books = scope.ServiceProvider.GetService<BookManager>();

                Book book;

                using (var task = new UploadTask())
                    book = await books.CreateAsync(new BookBase { Name = new[] { "name" } }, new BookContentBase(), task);

                var snapshots = await books.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Is.Not.Null.Or.Empty);
                Assert.That(snapshots, Has.Exactly(1).Items);
                Assert.That(snapshots[0].Event, Is.EqualTo(SnapshotEvent.Creation));
                Assert.That(snapshots[0].EntityId, Is.EqualTo(book.Id));
                Assert.That(snapshots[0].EntityType, Is.EqualTo(NanokaEntity.Book));
                Assert.That(snapshots[0].CommitterId, Is.EqualTo("userId"));

                var newBook = await books.UpdateAsync(book.Id, new BookBase { Name = new[] { "new name", "name 2" }, Category = BookCategory.ImageSet });

                Assert.That(newBook, Is.Not.Null);
                Assert.That(book.Id, Is.EqualTo(newBook.Id));
                Assert.That(newBook.Name, Is.Not.Null.Or.Empty);
                Assert.That(newBook.Name, Has.Exactly(2).Items);
                Assert.That(newBook.Name[0], Is.EqualTo("new name"));
                Assert.That(newBook.Category, Is.Not.EqualTo(book.Category));

                snapshots = await books.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Has.Exactly(2).Items);
                Assert.That(snapshots[1].Event, Is.EqualTo(SnapshotEvent.Modification));
                Assert.That(snapshots[1].EntityId, Is.EqualTo(book.Id));

                var revertedBook = await books.RevertAsync(book.Id, snapshots[0].Id);

                Assert.That(revertedBook, Is.Not.Null);
                Assert.That(revertedBook.Id, Is.EqualTo(book.Id));
                Assert.That(revertedBook.Name, Is.EqualTo(book.Name));
                Assert.That(revertedBook.Category, Is.EqualTo(book.Category));
                Assert.That(revertedBook.Category, Is.Not.EqualTo(newBook.Category));

                snapshots = await books.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Has.Exactly(3).Items);
                Assert.That(snapshots[2].Event, Is.EqualTo(SnapshotEvent.Rollback));
                Assert.That(snapshots[2].EntityId, Is.EqualTo(book.Id));

                await books.DeleteAsync(book.Id);

                Assert.That(Assert.ThrowsAsync<ResultException>(() => books.GetAsync(book.Id)).Result.Status, Is.EqualTo(404));

                snapshots = await books.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Has.Exactly(4).Items);
                Assert.That(snapshots[3].Event, Is.EqualTo(SnapshotEvent.Deletion));
                Assert.That(snapshots[3].EntityId, Is.EqualTo(book.Id));

                revertedBook = await books.RevertAsync(book.Id, snapshots[1].Id);

                Assert.That(revertedBook, Is.Not.Null);
                Assert.That(revertedBook.Name, Is.EqualTo(newBook.Name));
                Assert.That(revertedBook.Category, Is.EqualTo(newBook.Category));

                snapshots = await books.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Has.Exactly(5).Items);

                revertedBook = await books.RevertAsync(book.Id, snapshots[3].Id);

                Assert.That(revertedBook, Is.Null);
                Assert.That(Assert.ThrowsAsync<ResultException>(() => books.GetAsync(book.Id)).Result.Status, Is.EqualTo(404));
            }
        }

        [Test]
        public async Task SearchAsync()
        {
            using (var services = TestUtils.Services(
                c => c.Replace(ServiceDescriptor.Scoped<IUserClaims>(_ => new DummyUserClaimsProvider()))))

            using (var scope = services.CreateScope())
            {
                await services.GetService<INanokaDatabase>().MigrateAsync();

                var books = scope.ServiceProvider.GetService<BookManager>();

                // sample data
                using (var task = new UploadTask())
                    await books.CreateAsync(new BookBase { Name = new[] { "name 1" }, Category = BookCategory.Doujinshi, Rating = MaterialRating.Safe }, new BookContentBase { Language = LanguageType.English, IsColor = true }, task);

                using (var task = new UploadTask())
                {
                    using (var memory = new MemoryStream())
                    {
                        await task.AddFileAsync("1.jpg", memory, "image/jpeg");
                        await task.AddFileAsync("2.jpg", memory, "image/jpeg");
                    }

                    await books.CreateAsync(new BookBase { Name = new[] { "name 2" }, Category = BookCategory.Manga, Rating = MaterialRating.Questionable }, new BookContentBase { Language = LanguageType.Japanese, IsColor = false }, task);
                }

                using (var task = new UploadTask())
                {
                    using (var memory = new MemoryStream())
                        await task.AddFileAsync("1.jpg", memory, "image/jpeg");

                    await books.CreateAsync(new BookBase { Name = new[] { "name 3" }, Category = BookCategory.Manga, Rating = MaterialRating.Explicit }, new BookContentBase { Language = LanguageType.French, IsColor = false }, task);
                }

                //1
                var results = await books.SearchAsync(new BookQuery().WithName("name")
                                                                     .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(3).Items);

                //2
                results = await books.SearchAsync(new BookQuery().WithSorting(s => s.Ascending(BookSort.PageCount))
                                                                 .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(3).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 1"));
                Assert.That(results.Items[1].Name[0], Is.EqualTo("name 3"));
                Assert.That(results.Items[2].Name[0], Is.EqualTo("name 2"));

                //3
                results = await books.SearchAsync(new BookQuery().WithSorting(s => s.Descending(BookSort.PageCount))
                                                                 .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(3).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 2"));
                Assert.That(results.Items[1].Name[0], Is.EqualTo("name 3"));
                Assert.That(results.Items[2].Name[0], Is.EqualTo("name 1"));

                //4
                results = await books.SearchAsync(new BookQuery().WithSorting(s => s.Ascending(BookSort.PageCount))
                                                                 .WithCategory(BookCategory.Manga)
                                                                 .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(2).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 3"));
                Assert.That(results.Items[1].Name[0], Is.EqualTo("name 2"));

                //5
                results = await books.SearchAsync(new BookQuery().WithSorting(s => s.Ascending(BookSort.PageCount))
                                                                 .WithRating((MaterialRating.Explicit, MaterialRating.Safe))
                                                                 .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(2).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 1"));
                Assert.That(results.Items[1].Name[0], Is.EqualTo("name 3"));

                //6
                results = await books.SearchAsync(new BookQuery().WithLanguage(LanguageType.Japanese)
                                                                 .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(1).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 2"));

                //7
                results = await books.SearchAsync(new BookQuery().WithLanguage(LanguageType.Japanese)
                                                                 .WithRange(0, 0));

                Assert.That(results.Items, Has.Exactly(0).Items);

                //8
                results = await books.SearchAsync(new BookQuery().WithIsColor(true)
                                                                 .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(1).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 1"));
            }
        }
    }
}