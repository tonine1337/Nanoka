using System;
using System.IO;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
            services.AddMvc(m =>
                     {
                         // this filter ensures all responses are consistently wrapped in JSON
                         m.Filters.Add<PrimitiveResponseWrapperFilter>();
                     })
                    .AddApplicationPart(GetType().Assembly)
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
                    .AddScoped<IVoteRepository>(s => s.GetService<INanokaDatabase>());

            services.AddScoped(s => s.GetService<INanokaDatabase>() as ISoftDeleteQueue ?? new SoftDeleteQueue());

            services.AddScoped<SnapshotHelper>()
                    .AddScoped<VoteHelper>()
                    .AddScoped<TokenManager>();

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
                    new OpenApiInfo
                    {
                        Title   = "Nanoka API",
                        Version = "v1",
                        License = new OpenApiLicense
                        {
                            Name = "MIT",
                            Url  = new Uri("https://opensource.org/licenses/MIT")
                        }
                    });

                s.AddSecurityDefinition(
                    "Authorization",
                    new OpenApiSecurityScheme
                    {
                        Name        = "Authorization",
                        Type        = SecuritySchemeType.ApiKey,
                        In          = ParameterLocation.Header,
                        Scheme      = "Bearer",
                        Description = "JWT bearer token for authorization."
                    });

                s.OperationFilter<UserClaimsOperationFilter>();
                s.OperationFilter<VerifyHumanOperationFilter>();

                s.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Nanoka.xml"));
                s.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Nanoka.Models.xml"));

                s.EnableAnnotations();
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