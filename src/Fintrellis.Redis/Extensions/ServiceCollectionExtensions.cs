using Fintrellis.Redis.Interfaces;
using Fintrellis.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Fintrellis.Redis.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCachingService(this IServiceCollection services, string redisConnString)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(redisConnString);
            });

            services.AddSingleton<ICacheService, CacheService>();

            return services;
        }
    }
}
