using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nanoka.Models;
using NUnit.Framework;

namespace Nanoka.Tests
{
    [TestFixture]
    public class UserManagerTests
    {
        [SetUp]
        public async Task SetUpAsync() => await TestUtils.ResetDatabaseAsync();

        [Test]
        public async Task CreateAsync()
        {
            using (var services = TestUtils.Services(c => c.Replace(ServiceDescriptor.Scoped<IUserClaims>(_ => new DummyUserClaimsProvider()))))
            using (var scope = services.CreateScope())
            {
                var users = scope.ServiceProvider.GetService<UserManager>();

                await users.CreateAsync("testUser672", "securePassword1234");

                var user = await users.TryAuthenticateAsync("testUser672", "securePassword1234");

                Assert.That(user, Is.Not.Null);
                Assert.That(user.Username, Is.EqualTo("testUser672"));
                Assert.That(user.Secret, Is.Null);
                Assert.That(user.Permissions, Is.EqualTo(UserPermissions.None));
                Assert.That(user.Email, Is.Null);
                Assert.That(user.Restrictions, Is.Null.Or.Empty);
                Assert.That(user.Reputation, Is.Zero);

                Assert.That(user.Id, Is.EqualTo((await users.GetAsync(user.Id)).Id));
                Assert.That(await users.GetSnapshotsAsync(user.Id), Has.One.Items);
            }
        }
    }
}