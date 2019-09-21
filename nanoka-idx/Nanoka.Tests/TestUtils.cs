using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
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
                               .AddEnvironmentVariables()
                               .Build();

            var environment = new HostingEnvironment();

            services.AddSingleton<IHostingEnvironment>(environment);

            new Startup(configuration, environment).ConfigureServices(services);

            configure?.Invoke(services);

            return services.BuildServiceProvider();
        }

        static IConfigurationSource CreateTestConfiguration() => new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string>
            {
                // use in-memory storage
                { "Storage:Type", "Memory" },

                // use a random prefix to avoid clashing between tests
                { "Elastic:IndexPrefix", $"nanoka-test-{Extensions.RandomString(10)}-" }
            }
        };
    }
}