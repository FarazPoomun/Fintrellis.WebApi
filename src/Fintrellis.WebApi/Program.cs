using Fintrellis.MongoDb.Extensions;
using Fintrellis.Services.Interfaces;
using Fintrellis.Services.Mapping;
using Fintrellis.Services.Services;
using MongoDB.Driver;

namespace Fintrellis.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var serviceAssembly = typeof(PostMapping).Assembly;

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddTransient<IPostService, PostService>();
            builder.Services.RegisterRepository();
            builder.Services.AddAutoMapper(serviceAssembly);

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
