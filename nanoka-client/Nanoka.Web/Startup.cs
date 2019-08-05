using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Web.Database;
using Newtonsoft.Json;

namespace Nanoka.Web
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // options
            services.Configure<NanokaOptions>(_configuration);

            // mvc
            services.AddMvc();

            // database
            services.AddSingleton<NanokaDatabase>();

            // other utility
            services.AddSingleton<JsonSerializer>()
                    .AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // development page
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            // mvc
            app.UseMvc();
        }
    }
}