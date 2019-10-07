using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nanoka.Controllers;
using Nanoka.Database;
using Nanoka.Models;
using Nanoka.Models.Requests;
using NUnit.Framework;

namespace Nanoka.Tests
{
    [TestFixture]
    public class BookControllerTests
    {
        [Test]
        public async Task CreateAsync()
        {
            using (var services = TestUtils.Services(
                c => c.Replace(ServiceDescriptor.Scoped<IUserClaims>(_ => new DummyUserClaimsProvider()))))

            using (var scope = services.CreateScope())
            {
                await services.GetService<INanokaDatabase>().MigrateAsync();

                var controller = scope.ServiceProvider.GetService<BookController>();

                var uploadId = (await controller.CreateUploadAsync(new CreateNewBookRequest
                {
                    Book = new BookBase
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
                    },
                    Content = new BookContentBase
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
                    }
                })).Value.Id;

                using (var stream = TestUtils.DummyImage())
                    await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                var book = (await controller.DeleteUploadAsync(uploadId, true)).Value;

                Assert.That(book, Is.Not.Null);
                Assert.That(book.Name, Is.Not.Null.Or.Empty);
                Assert.That(book.Name, Has.Exactly(2).Items);
                Assert.That(book.Name[0], Is.EqualTo("my book"));
                Assert.That(book.Tags, Is.Not.Null.Or.Empty);
                Assert.That(book.Tags, Has.Exactly(2).Items);
                Assert.That(book.Tags, Contains.Key(BookTag.Artist));
                Assert.That(book.Tags[BookTag.Artist], Has.Exactly(2).Items);
                Assert.That(book.Tags[BookTag.Artist][1], Is.EqualTo("artist 2"));
                Assert.That(book.Contents, Has.One.Items);

                var content = (await controller.GetContentAsync(book.Id, book.Contents[0].Id)).Value;

                Assert.That(content, Is.Not.Null);
                Assert.That(content.Id, Is.EqualTo(book.Contents[0].Id));
                Assert.That(content.PageCount, Is.EqualTo(1));
                Assert.That(content.Language, Is.EqualTo(LanguageType.English));
                Assert.That(content.IsColor, Is.False);
                Assert.That(content.Sources, Is.Not.Null.Or.Empty);
                Assert.That(content.Sources, Has.Exactly(1).Items);
                Assert.That(content.Sources[0].Website, Is.EqualTo("google.com"));

                uploadId = (await controller.CreateUploadAsync(new CreateNewBookRequest
                {
                    BookId = book.Id,
                    Content = new BookContentBase
                    {
                        Language = LanguageType.ChineseSimplified,
                        IsColor  = true
                    }
                })).Value.Id;

                using (var stream = TestUtils.DummyImage())
                    await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                using (var stream = TestUtils.DummyImage())
                    await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                using (var stream = TestUtils.DummyImage())
                    await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                var book2 = (await controller.DeleteUploadAsync(uploadId, true)).Value;

                Assert.That(book2, Is.Not.Null);
                Assert.That(book2.Id, Is.EqualTo(book.Id));
                Assert.That(book2.Name, Is.EqualTo(book.Name));
                Assert.That(book2.Contents.Length, Is.EqualTo(2));

                var content2 = (await controller.GetContentAsync(book.Id, book2.Contents[2].Id)).Value;

                Assert.That(content2, Is.Not.Null);
                Assert.That(content2.Id, Is.EqualTo(book2.Contents[2].Id));
                Assert.That(content2.PageCount, Is.EqualTo(3));
                Assert.That(content2.IsColor, Is.True);
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

                var controller = scope.ServiceProvider.GetService<BookController>();

                var uploadId = (await controller.CreateUploadAsync(new CreateNewBookRequest
                {
                    Book    = new BookBase { Name = new[] { "name" } },
                    Content = new BookContentBase()
                })).Value.Id;

                using (var stream = TestUtils.DummyImage())
                    await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                var book = (await controller.DeleteUploadAsync(uploadId, true)).Value;

                var snapshots = await controller.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Is.Not.Null.Or.Empty);
                Assert.That(snapshots, Has.Exactly(1).Items);
                Assert.That(snapshots[0].Event, Is.EqualTo(SnapshotEvent.Creation));
                Assert.That(snapshots[0].EntityId, Is.EqualTo(book.Id));
                Assert.That(snapshots[0].EntityType, Is.EqualTo(NanokaEntity.Book));
                Assert.That(snapshots[0].CommitterId, Is.EqualTo("userId"));

                var newBook = (await controller.UpdateAsync(book.Id, new BookBase { Name = new[] { "new name", "name 2" }, Category = BookCategory.ImageSet })).Value;

                Assert.That(newBook, Is.Not.Null);
                Assert.That(book.Id, Is.EqualTo(newBook.Id));
                Assert.That(newBook.Name, Is.Not.Null.Or.Empty);
                Assert.That(newBook.Name, Has.Exactly(2).Items);
                Assert.That(newBook.Name[0], Is.EqualTo("new name"));
                Assert.That(newBook.Category, Is.Not.EqualTo(book.Category));

                snapshots = await controller.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Has.Exactly(2).Items);
                Assert.That(snapshots[1].Event, Is.EqualTo(SnapshotEvent.Modification));
                Assert.That(snapshots[1].EntityId, Is.EqualTo(book.Id));

                var revertedBook = (await controller.RevertAsync(book.Id, new RevertEntityRequest { SnapshotId = snapshots[0].Id })).Value;

                Assert.That(revertedBook, Is.Not.Null);
                Assert.That(revertedBook.Id, Is.EqualTo(book.Id));
                Assert.That(revertedBook.Name, Is.EqualTo(book.Name));
                Assert.That(revertedBook.Category, Is.EqualTo(book.Category));
                Assert.That(revertedBook.Category, Is.Not.EqualTo(newBook.Category));

                snapshots = await controller.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Has.Exactly(3).Items);
                Assert.That(snapshots[2].Event, Is.EqualTo(SnapshotEvent.Rollback));
                Assert.That(snapshots[2].EntityId, Is.EqualTo(book.Id));

                await controller.DeleteAsync(book.Id);

                Assert.That((await controller.GetAsync(book.Id)).Result, Is.TypeOf<NotFoundObjectResult>());

                snapshots = await controller.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Has.Exactly(4).Items);
                Assert.That(snapshots[3].Event, Is.EqualTo(SnapshotEvent.Deletion));
                Assert.That(snapshots[3].EntityId, Is.EqualTo(book.Id));

                revertedBook = (await controller.RevertAsync(book.Id, new RevertEntityRequest { SnapshotId = snapshots[1].Id })).Value;

                Assert.That(revertedBook, Is.Not.Null);
                Assert.That(revertedBook.Name, Is.EqualTo(newBook.Name));
                Assert.That(revertedBook.Category, Is.EqualTo(newBook.Category));

                snapshots = await controller.GetSnapshotsAsync(book.Id);

                Assert.That(snapshots, Has.Exactly(5).Items);

                revertedBook = (await controller.RevertAsync(book.Id, new RevertEntityRequest { SnapshotId = snapshots[3].Id })).Value;

                Assert.That(revertedBook, Is.Null);
                Assert.That((await controller.GetAsync(book.Id)).Result, Is.TypeOf<NotFoundObjectResult>());
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

                var controller = scope.ServiceProvider.GetService<BookController>();

                // sample data
                {
                    var uploadId = (await controller.CreateUploadAsync(new CreateNewBookRequest
                    {
                        Book = new BookBase
                        {
                            Name     = new[] { "name 1" },
                            Category = BookCategory.Doujinshi,
                            Rating   = MaterialRating.Safe
                        },
                        Content = new BookContentBase
                        {
                            Language = LanguageType.English,
                            IsColor  = true
                        }
                    })).Value.Id;

                    // 1 image
                    using (var stream = TestUtils.DummyImage())
                        await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                    await controller.DeleteUploadAsync(uploadId, true);
                }

                {
                    var uploadId = (await controller.CreateUploadAsync(new CreateNewBookRequest
                    {
                        Book = new BookBase
                        {
                            Name     = new[] { "name 2" },
                            Category = BookCategory.Manga,
                            Rating   = MaterialRating.Questionable
                        },
                        Content = new BookContentBase
                        {
                            Language = LanguageType.Japanese,
                            IsColor  = false
                        }
                    })).Value.Id;

                    // 3 images
                    using (var stream = TestUtils.DummyImage())
                        await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                    using (var stream = TestUtils.DummyImage())
                        await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                    using (var stream = TestUtils.DummyImage())
                        await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                    await controller.DeleteUploadAsync(uploadId, true);
                }

                {
                    var uploadId = (await controller.CreateUploadAsync(new CreateNewBookRequest
                    {
                        Book = new BookBase
                        {
                            Name     = new[] { "name 3" },
                            Category = BookCategory.Manga,
                            Rating   = MaterialRating.Explicit
                        },
                        Content = new BookContentBase
                        {
                            Language = LanguageType.French,
                            IsColor  = false
                        }
                    })).Value.Id;

                    // 2 images
                    using (var stream = TestUtils.DummyImage())
                        await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                    using (var stream = TestUtils.DummyImage())
                        await controller.UploadFileAsync(uploadId, stream.AsFormFile());

                    await controller.DeleteUploadAsync(uploadId, true);
                }

                //1
                var results = await controller.SearchAsync(new BookQuery().WithName("name")
                                                                          .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(3).Items);

                //2
                results = await controller.SearchAsync(new BookQuery().WithSorting(s => s.Ascending(BookSort.PageCount))
                                                                      .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(3).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 1"));
                Assert.That(results.Items[1].Name[0], Is.EqualTo("name 3"));
                Assert.That(results.Items[2].Name[0], Is.EqualTo("name 2"));

                //3
                results = await controller.SearchAsync(new BookQuery().WithSorting(s => s.Descending(BookSort.PageCount))
                                                                      .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(3).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 2"));
                Assert.That(results.Items[1].Name[0], Is.EqualTo("name 3"));
                Assert.That(results.Items[2].Name[0], Is.EqualTo("name 1"));

                //4
                results = await controller.SearchAsync(new BookQuery().WithSorting(s => s.Ascending(BookSort.PageCount))
                                                                      .WithCategory(BookCategory.Manga)
                                                                      .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(2).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 3"));
                Assert.That(results.Items[1].Name[0], Is.EqualTo("name 2"));

                //5
                results = await controller.SearchAsync(new BookQuery().WithSorting(s => s.Ascending(BookSort.PageCount))
                                                                      .WithRating((MaterialRating.Explicit, MaterialRating.Safe))
                                                                      .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(2).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 1"));
                Assert.That(results.Items[1].Name[0], Is.EqualTo("name 3"));

                //6
                results = await controller.SearchAsync(new BookQuery().WithLanguage(LanguageType.Japanese)
                                                                      .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(1).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 2"));

                //7
                results = await controller.SearchAsync(new BookQuery().WithLanguage(LanguageType.Japanese)
                                                                      .WithRange(0, 0));

                Assert.That(results.Items, Has.Exactly(0).Items);

                //8
                results = await controller.SearchAsync(new BookQuery().WithIsColor(true)
                                                                      .WithRange(0, 3));

                Assert.That(results.Items, Has.Exactly(1).Items);
                Assert.That(results.Items[0].Name[0], Is.EqualTo("name 1"));
            }
        }
    }
}