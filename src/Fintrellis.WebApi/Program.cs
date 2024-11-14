using Fintrellis.MongoDb.Extensions;
using Fintrellis.MongoDb.Interfaces;
using Fintrellis.MongoDb.Services;
using Fintrellis.Redis.Extensions;
using Fintrellis.Services.Interfaces;
using Fintrellis.Services.Mapping;
using Fintrellis.Services.Resiliency;
using Fintrellis.Services.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using MongoDB.Driver;

namespace Fintrellis.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var serviceAssembly = typeof(PostMapping).Assembly;
            var retryPolicyConfiguration = builder.Configuration.GetSection("RetryPolicy");
            var retryAttempt = retryPolicyConfiguration.GetValue<int>("RetryAttempt");
            var incrementalCount = retryPolicyConfiguration.GetValue<int>("IncrementalCount");
            var redisConfiguration = builder.Configuration.GetSection("Redis");
            var redisConfigurationConnString = redisConfiguration.GetValue<string>("ConnectionString");

            builder.Services.AddControllers();

            builder.Services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(serviceAssembly);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.RegisterRepository();
            builder.Services.AddSingleton(typeof(ICachedRepository<>), typeof(CachedRepository<>));
            builder.Services.AddTransient<IPostService, PostService>();

            builder.Services.RegisterCachingService(redisConfigurationConnString!);
            builder.Services.AddAutoMapper(serviceAssembly);
            builder.Services.AddSingleton<IRetryHandler>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<PollyRetryHandler>>();
                return new PollyRetryHandler(retryAttempt, incrementalCount, logger);
            });

            ConfigureMongo(builder);
            var app = builder.Build();

            // Always show swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fintrellis Web App V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void ConfigureMongo(WebApplicationBuilder builder)
        {
            var mongoSettings = builder.Configuration.GetSection("MongoDB");
            var mongoConnectionString = mongoSettings.GetValue<string>("ConnectionString");
            var mongoDatabaseName = mongoSettings.GetValue<string>("DatabaseName");

            var client = new MongoClient(mongoConnectionString);
            var database = client.GetDatabase(mongoDatabaseName);

            builder.Services.AddSingleton(database);
            builder.Services.AddSingleton(typeof(IMongoClient), p => client);
        }
    }
}
