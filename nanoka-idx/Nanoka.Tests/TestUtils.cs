using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Database;

namespace Nanoka.Tests
{
    public static class TestUtils
    {
        public static ServiceProvider Services(Action<IServiceCollection> configure = null)
        {
            var services = new ServiceCollection();

            new Startup(new ConfigurationBuilder().Build(), new HostingEnvironment()).ConfigureServices(services);

            configure?.Invoke(services);

            return services.BuildServiceProvider();
        }

        public static async Task ResetDatabaseAsync()
        {
            using (var services = Services())
                await services.GetService<INanokaDatabase>().ResetAsync();
        }
    }
}