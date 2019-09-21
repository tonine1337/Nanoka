using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nanoka.Database;
using Nanoka.Models;
using NUnit.Framework;

namespace Nanoka.Tests
{
    [TestFixture]
    public class UserManagerTests
    {
        [Test]
        public async Task CreateAsync()
        {
            using (var services = TestUtils.Services(c => c.Replace(ServiceDescriptor.Scoped<IUserClaims>(_ => new DummyUserClaimsProvider()))))
            using (var scope = services.CreateScope())
            {
                await services.GetService<INanokaDatabase>().MigrateAsync();

                var users = scope.ServiceProvider.GetService<UserManager>();

                var user = await users.CreateAsync("testUser672", "securePassword1234");

                Assert.That(user, Is.Not.Null);
                Assert.That(user.Username, Is.EqualTo("testUser672"));
                Assert.That(user.Secret, Is.Null);
                Assert.That(user.Permissions, Is.EqualTo(UserPermissions.None));
                Assert.That(user.Email, Is.Null);
                Assert.That(user.Restrictions, Is.Null.Or.Empty);
                Assert.That(user.Reputation, Is.Zero);

                Assert.That(user.Id, Is.EqualTo((await users.TryAuthenticateAsync("testUser672", "securePassword1234")).Id));
                Assert.That(user.Id, Is.EqualTo((await users.GetAsync(user.Id)).Id));

                var snapshots = await users.GetSnapshotsAsync(user.Id);
                Assert.That(snapshots, Has.One.Items);
                Assert.That(snapshots[0].Event, Is.EqualTo(SnapshotEvent.Creation));

                //todo: user update test
            }
        }
    }
}