using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Web.Database;

namespace Nanoka.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                                 .UseStartup<Startup>();

            using (var host = builder.Build())
            {
                // migrate database
                await host.Services.GetService<NanokaDatabase>().MigrateAsync();

                // run host
                await host.RunAsync();
            }
        }
    }
}