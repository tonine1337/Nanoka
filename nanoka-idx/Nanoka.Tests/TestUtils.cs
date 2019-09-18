using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Nanoka.Tests
{
    public static class TestUtils
    {
        public static ServiceProvider Services(Action<IServiceCollection> configure = null)
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                               .Add(CreateTestConfiguration())
                               .Build();

            new Startup(configuration, new HostingEnvironment()).ConfigureServices(services);

            configure?.Invoke(services);

            return services.BuildServiceProvider();
        }

        static IConfigurationSource CreateTestConfiguration() => new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string>
            {
                // use a random prefix to avoid clashing between tests
                { "Elastic:IndexPrefix", $"nanoka-test-{Extensions.RandomString(10)}-" }
            }
        };
    }
}