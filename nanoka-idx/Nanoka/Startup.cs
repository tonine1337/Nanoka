using System;
using System.IO;
using System.Reflection;
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
using Swashbuckle.AspNetCore.Swagger;

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
                    .AddScoped<IUserRepository>(s => s.GetService<INanokaDatabase>())
                    .AddScoped<IBookRepository>(s => s.GetService<INanokaDatabase>())
                    .AddScoped<IImageRepository>(s => s.GetService<INanokaDatabase>())
                    .AddScoped<ISnapshotRepository>(s => s.GetService<INanokaDatabase>())
                    .AddScoped<IVoteRepository>(s => s.GetService<INanokaDatabase>())
                    .AddScoped<IDeleteFileRepository>(s => s.GetService<INanokaDatabase>());

            services.AddScoped<SnapshotHelper>()
                    .AddScoped<UserManager>()
                    .AddScoped<TokenManager>()
                    .AddScoped<VoteManager>();

            // storage
            services.AddSingleton<IStorage>(s => new StorageWrapper(s, _configuration.GetSection("Storage")))
                    .AddMemoryCache()
                    .AddDistributedMemoryCache();

            // uploader
            services.AddScoped<UploadManager>()
                    .AddSingleton<UploadTaskCollection>()
                    .AddHostedService<UploadAutoExpiryJob>();

            // swagger docs
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc(
                    "v1",
                    new Info
                    {
                        Title   = "Nanoka API",
                        Version = "v1",
                        License = new License
                        {
                            Name = "MIT",
                            Url  = "https://opensource.org/licenses/MIT"
                        }
                    });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                s.IncludeXmlComments(xmlPath);
            });

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

            // swagger docs
            app.UseSwagger(s => s.RouteTemplate = "/docs/{documentName}.json");

            // mvc
            app.UseMvc();
        }
    }
}