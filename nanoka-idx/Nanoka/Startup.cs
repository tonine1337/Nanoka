using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nanoka.Database;
using Nanoka.Models;
using Nanoka.Storage;

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
                    .Configure<RecaptchaOptions>(_configuration.GetSection("reCAPTCHA"));

            // mvc
            services.AddMvc()
                    .AddControllersAsServices()
                    .AddJsonOptions(j => NanokaJsonSerializer.Apply(j.SerializerSettings));

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

            services.AddScoped<IUserClaims, HttpUserClaimsProvider>();

            // database
            services.AddSingleton<INanokaDatabase, NanokaElasticDatabase>()
                    .AddScoped<SnapshotManager>()
                    .AddScoped<UserManager>()
                    .AddScoped<TokenManager>()
                    .AddScoped<BookManager>()
                    .AddScoped<VoteManager>();

            // storage
            services.AddSingleton<IStorage>(s => new StorageWrapper(s, _configuration.GetSection("Storage")))
                    .AddDistributedMemoryCache();

            // uploader
            services.AddScoped<UploadManager>()
                    .AddSingleton<UploadTaskCollection>()
                    .AddHostedService<UploadAutoExpiryJob>();

            // other utilities
            services.AddSingleton(NanokaJsonSerializer.Create())
                    .AddHttpClient()
                    .AddHttpContextAccessor()
                    .AddAutoMapper(typeof(ModelMapperProfile))
                    .AddSingleton<RecaptchaValidator>()
                    .AddSingleton<ImageProcessor>()
                    .AddSingleton<ILocker, NamedResourceLocker>()
                    .AddSingleton<PasswordHashHelper>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // cors
            app.UseCors(o => o.AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowAnyOrigin());

            // global exception handling
            app.UseExceptionHandler("/error")
               .UseStatusCodePagesWithReExecute("/error/{0}");

            // development page
            /*if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();*/

            // authentication
            app.UseAuthentication()
               .UseMiddleware<TokenValidatingMiddleware>();

            // mvc
            app.UseMvc();
        }
    }
}