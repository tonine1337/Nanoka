using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanoka.Core.Installer;

namespace Nanoka.Core
{
    public class NanokaStartup : StartupBase
    {
        public static async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var host = new WebHostBuilder()
                      .UseContentRoot(Environment.CurrentDirectory)
                      .UseKestrel(kestrel =>
                       {
                           kestrel.Listen(IPAddress.Loopback,
                                          7230,
                                          o => o.UseHttps(NanokaCrt.Load()));
                       })
                      .ConfigureAppConfiguration((hostingContext, config) =>
                       {
                           var env = hostingContext.HostingEnvironment;

                           config.AddJsonFile("appsettings.json", true, true)
                                 .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                                 .AddJsonFile("appsettings.UserOverride.json", true, true)
                                 .AddEnvironmentVariables();
                       })
                      .ConfigureLogging((hostingContext, logging) =>
                       {
                           logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                                  .AddConsole();
                       })
                      .UseDefaultServiceProvider((context, options) =>
                       {
                           options.ValidateScopes =
                               context.HostingEnvironment.IsDevelopment();
                       })
                      .UseStartup<NanokaStartup>()
                      .Build();

            await host.RunAsync(cancellationToken);
        }

        readonly IConfiguration _configuration;

        public NanokaStartup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            // kestrel settings
            services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelServerOptionsSetup>();

            // lightweight mvc
            services.AddMvcCore();

            // options
            services.Configure<NanokaOptions>(_configuration);
            services.Configure<IpfsOptions>(_configuration.GetSection("Ipfs"));
        }

        public override void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}