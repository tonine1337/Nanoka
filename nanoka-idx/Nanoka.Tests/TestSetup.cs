using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Database;
using NUnit.Framework;

// all tests are parallelizable
[assembly: Parallelizable(ParallelScope.Children)]

namespace Nanoka.Tests
{
    [SetUpFixture]
    public class TestSetup
    {
        /// <summary>
        /// Resets the database after all tests have finished.
        /// </summary>
        [OneTimeTearDown]
        public async Task TearDownAsync()
        {
            using (var services = TestUtils.Services())
                await services.GetService<INanokaDatabase>().ResetAsync();
        }
    }
}
