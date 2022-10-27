using DBScriptDeployment.Queries;
using DBScriptDeployment.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

namespace DBScriptDeployment
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            var key = Encoding.ASCII.GetBytes(Configuration["IdentitySettings:Secret"]);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddAuthorization();

            services.AddSingleton<ITFSQueries, TFSQueries>(serviceProvider =>
            {
                return new TFSQueries(Configuration["TFS:ConnectionString"]);
            });

            services.AddScoped<IDBService, DBService>(serviceProvider =>
            {
                return new DBService(serviceProvider.GetRequiredService<ILogger<DBService>>());

            });

            services.AddScoped<IUserService, UserService>(serviceProvider =>
            {
                return new UserService(Configuration["IdentitySettings:Secret"]);
            });

            services.AddHttpClient<ITFSApiClient, TFSApiClient>();

            services.AddControllers();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Database Script Deploy",
                    Version = "v1",
                    Description = "The Database Script Deploy HTTP API"
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            app.UseSwagger()
                .UseSwaggerUI(setup =>
                {
                    setup.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "DBScriptDeployment.API V1");
                });

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors("CorsPolicy");
            app.UseStaticFiles();

            app.UseEndpoints(endpoins =>
            {
                endpoins.MapDefaultControllerRoute();
                endpoins.MapControllers();
            });
        }
    }
}
