﻿using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Database;
using Nanoka.Storage;

namespace Nanoka
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                                 .UseStartup<Startup>();

            using (var host = builder.Build())
            {
                // initialize storage
                await host.Services.GetService<IStorage>().InitializeAsync();

                // migrate database
                await host.Services.GetService<INanokaDatabase>().MigrateAsync();

                // run host
                await host.RunAsync();
            }
        }
    }
}