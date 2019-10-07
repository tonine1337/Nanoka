using System;
using System.Threading.Tasks;
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
    public class UserControllerTests
    {
        [Test]
        public async Task CreateAsync()
        {
            using (var services = TestUtils.Services(
                c => c.Replace(ServiceDescriptor.Scoped<IUserClaims>(_ => new DummyUserClaimsProvider()))))

            using (var scope = services.CreateScope())
            {
                await services.GetService<INanokaDatabase>().MigrateAsync();

                var controller = scope.ServiceProvider.GetService<UserController>();

                var user = (await controller.RegisterAsync(new RegistrationRequest
                {
                    Username = "testUser672",
                    Password = "securePassword1234"
                })).Value.User;

                Assert.That(user, Is.Not.Null);

                var id = user.Id;

                var authResponse = (await controller.AuthentiateAsync(new AuthenticationRequest
                {
                    Username = "testUser672",
                    Password = "securePassword1234"
                })).Value;

                Assert.That(authResponse.AccessToken, Is.Not.Null);
                Assert.That(authResponse.Expiry, Is.GreaterThan(DateTime.UtcNow));

                user = (await controller.GetAsync(id)).Value;

                Assert.That(user, Is.Not.Null);
                Assert.That(user.Id, Is.EqualTo(id));
                Assert.That(user.Username, Is.EqualTo("testUser672"));
                Assert.That(user.Secret, Is.Null);
                Assert.That(user.Permissions, Is.EqualTo(UserPermissions.None));
                Assert.That(user.Email, Is.Null);
                Assert.That(user.Restrictions, Is.Null.Or.Empty);
                Assert.That(user.Reputation, Is.Zero);

                var snapshots = await controller.GetSnapshotsAsync(user.Id);

                Assert.That(snapshots, Has.One.Items);
                Assert.That(snapshots[0].Event, Is.EqualTo(SnapshotEvent.Creation));
                Assert.That(snapshots[0].EntityType, Is.EqualTo(NanokaEntity.User));
                Assert.That(snapshots[0].EntityId, Is.EqualTo(user.Id));
                Assert.That(snapshots[0].Value, Is.Not.Null);
                Assert.That(snapshots[0].Value.Id, Is.EqualTo(user.Id));

                //todo: user update test
            }
        }
    }
}