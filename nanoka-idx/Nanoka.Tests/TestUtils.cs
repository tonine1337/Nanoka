using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

            services.AddSingleton<IHostingEnvironment>(environment)
                    .AddLogging(logging => logging.AddConsole());

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
                { "Elastic:IndexPrefix", $"nanoka-test-{Snowflake.New}-" }
            }
        };

        public static IFormFile AsFormFile(this Stream stream, string name = null)
            => new FormFile(stream, 0, stream.Length, name, name);

        public static Stream DummyImage()
        {
            var memory = new MemoryStream();

            using (var image = new Image<Rgba32>(1, 1))
                image.SaveAsPng(memory);

            memory.Position = 0;

            return memory;
        }
    }
}