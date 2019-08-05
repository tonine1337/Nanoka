using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanoka.Core.Client;
using Nanoka.Core.Installer;
using Newtonsoft.Json;

namespace Nanoka.Client
{
    public class Startup : StartupBase
    {
        public static async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var builder = new WebHostBuilder()
                         .UseContentRoot(Environment.CurrentDirectory)
                         .UseKestrel(kestrel =>
                          {
                              kestrel.Listen(IPAddress.Loopback,
                                             Localhost.Port,
                                             o => o.UseHttps(NanokaCrt.GetCertificate()));
                          })
                         .ConfigureAppConfiguration((hostingContext, config) =>
                          {
                              var env = hostingContext.HostingEnvironment;

                              config.AddJsonFile("settings.json", true, true)
                                    .AddJsonFile($"settings.{env.EnvironmentName}.json", true, true)
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
                         .UseStartup<Startup>();

            using (var host = builder.Build())
            {
                // start ipfs daemon
                await host.Services.GetService<IpfsManager>().StartDaemonAsync(cancellationToken);

                // connect to database
                await host.Services.GetService<IDatabaseClient>().ConnectAsync(cancellationToken);

                // run host
                await host.RunAsync(cancellationToken);
            }
        }

        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            // kestrel settings
            services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelServerOptionsSetup>();

            // lightweight mvc
            services.AddMvcCore(options =>
                     {
                         // don't ever need browsers to cache our response
                         options.Filters.Add(new ResponseCacheAttribute
                         {
                             NoStore  = true,
                             Location = ResponseCacheLocation.None
                         });
                     })
                    .AddApiExplorer()
                    .AddJsonFormatters()
                    .AddControllersAsServices()
                    .AddCors(cors => cors.AddDefaultPolicy(policy => policy.AllowAnyHeader()
                                                                           .AllowAnyMethod()
                                                                           .AllowAnyOrigin()));

            // options
            services.Configure<NanokaOptions>(_configuration)
                    .Configure<IpfsOptions>(_configuration.GetSection("Ipfs"))
                    .Configure<DatabaseOptions>(_configuration.GetSection("Database"));

            // ipfs subsystem
            services.AddSingleton<IpfsClient>()
                    .AddSingleton<IpfsManager>();

            // database
            services.AddSingleton<IDatabaseClient, DatabaseClient>();

            // other stuff
            services.AddSingleton<JsonSerializer>()
                    .AddHttpClient();
        }

        public override void Configure(IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetService<ILogger<Startup>>();

            logger.LogInformation($"Nanoka client server: {Localhost.Url()}");

            // global exception handling
            app.UseExceptionHandler("/errors/500")
               .UseStatusCodePagesWithReExecute("/errors/{0}");

            // development page
            if (app.ApplicationServices.GetService<IHostingEnvironment>().IsDevelopment())
                app.UseDeveloperExceptionPage();

            // mvc
            app.UseMvc();
        }
    }
}
