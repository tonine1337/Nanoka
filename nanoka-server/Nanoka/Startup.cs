using System.Text;
using AutoMapper;
using Ipfs.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nanoka.Database;
using Newtonsoft.Json;

namespace Nanoka
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
            var options = _configuration.Get<NanokaOptions>();

            // options
            services.Configure<NanokaOptions>(_configuration);

            // mvc
            services.AddMvc()
                    .AddControllersAsServices();

            services.AddAuthentication(a =>
                     {
                         a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                         a.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                     })
                    .AddJwtBearer(j =>
                     {
                         var secret = Encoding.Default.GetBytes(options.Secret);

                         j.TokenValidationParameters = new TokenValidationParameters
                         {
                             ValidateIssuerSigningKey = true,
                             IssuerSigningKey         = new SymmetricSecurityKey(secret),
                             ValidateIssuer           = false,
                             ValidateAudience         = false
                         };
                     });

            // database
            services.AddSingleton<NanokaDatabase>();

            // ipfs
            services.AddSingleton<IpfsClient>()
                    .AddHostedService<IpfsManager>();

            // background services
            services.AddHostedDependencyService<UploadManager>();

            // other utility
            services.AddSingleton<JsonSerializer>()
                    .AddHttpClient()
                    .AddAutoMapper(typeof(ModelMapperProfile));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // global exception handling
            app.UseExceptionHandler("/error")
               .UseStatusCodePagesWithReExecute("/error/{0}");

            // development page
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseCors(o => o.AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowAnyOrigin());

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}