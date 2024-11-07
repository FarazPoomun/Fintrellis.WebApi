using Fintrellis.MongoDb.Interfaces;
using Fintrellis.MongoDb.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fintrellis.MongoDb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterRepository(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));
            return services;
        }
    }
}
