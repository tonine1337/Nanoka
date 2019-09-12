using System;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nanoka.Database;
using Nanoka.Storage;
using Newtonsoft.Json;

namespace Nanoka
{
    public class Startup
    {
        readonly IConfiguration _configuration;
        readonly IHostingEnvironment _environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment   = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var options = _configuration.Get<NanokaOptions>();

            // options
            services.Configure<NanokaOptions>(_configuration)
                    .Configure<ElasticOptions>(_configuration.GetSection("Elastic"))
                    .Configure<LocalStorageOptions>(_configuration.GetSection("Storage"))
                    .Configure<B2Options>(_configuration.GetSection("Storage"))
                    .Configure<RecaptchaOptions>(_configuration.GetSection("reCAPTCHA"));

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

            services.AddScoped<UserClaimSet>();

            // database
            services.AddSingleton<INanokaDatabase, NanokaElasticDatabase>()
                    .AddScoped<SnapshotManager>()
                    .AddScoped<UserManager>()
                    .AddScoped<BookManager>()
                    .AddScoped<VoteManager>();

            // storage
            var storage = _configuration.GetSection("Storage")["Type"];

            switch (storage?.ToLowerInvariant())
            {
                case null:
                case "":
                case "local":
                    services.AddSingleton<IStorage, LocalStorage>();
                    break;

                case "b2":
                    services.AddSingleton<IStorage, B2Storage>();
                    break;

                default: throw new NotSupportedException($"Unsupported storage type '{storage}'.");
            }

            services.AddScoped<UploadManager>()
                    .AddSingleton<UploadTaskCollection>();

            // other utilities
            services.AddSingleton<JsonSerializer>()
                    .AddSingleton<NamedLocker>()
                    .AddHttpClient()
                    .AddAutoMapper(typeof(ModelMapperProfile))
                    .AddScoped<RecaptchaValidator>()
                    .AddScoped<ImageProcessor>()
                    .AddHttpContextAccessor()
                    .AddSingleton<PasswordHashHelper>();
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