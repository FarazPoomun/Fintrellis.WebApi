using Fintrellis.MongoDb.Interfaces;
using Fintrellis.Redis.Interfaces;
using Fintrellis.Services.Interfaces;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Fintrellis.Services.Services
{
    /// <inheritdoc/>
    public class CachedRepository<T>(IRepository<T> repository, ICacheService cacheService) : ICachedRepository<T> where T : class, IEntity
    {
        /// <inheritdoc/>
        public async Task InsertOneAsync(string cacheId, T entity)
        {
            await repository.InsertOneAsync(entity);
            await CacheEntityAsync(cacheId, entity);
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetAllAsync(string? cacheId = null, Expression<Func<T, bool>>? predicate = null)
        {
            if (!string.IsNullOrEmpty(cacheId))
            {
                // Only check cache if cacheId is provided 
                // redis doesnt provide a native way to get all
                var cachedEntity = await cacheService.GetAsync<T>(cacheId);

                if (cachedEntity != null)
                {
                    return [cachedEntity];
                }
            }

            var entities = await repository.GetAllAsync(predicate);

            if (cacheId != null && entities.Count > 0)
            {
                await CacheEntityAsync(cacheId, entities.First());
            }

            return entities ?? [];
        }

        /// <inheritdoc/>
        public async Task<T?> GetFirstOrDefaultAsync(string cacheId, Expression<Func<T, bool>>? predicate = null)
        {
            var cachedEntity = await cacheService.GetAsync<T>(cacheId);

            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            var entity = await repository.GetFirstOrDefaultAsync(predicate);
            if (entity != null)
            {
                await CacheEntityAsync(cacheId, entity);
            }
            return entity;
        }

        /// <inheritdoc/>
        public async Task DeleteOneAsync(string cacheId, Expression<Func<T, bool>> predicate)
        {
            await repository.DeleteOneAsync(predicate);
            await cacheService.RemoveData(cacheId);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(string cacheId, T entity)
        {
            await repository.UpdateAsync(entity);
            await cacheService.RemoveData(cacheId);
            await CacheEntityAsync(cacheId, entity);
        }

        private async Task CacheEntityAsync(string cacheId,T entity)
        {
            await cacheService.SetAsync(cacheId, entity, DateTimeOffset.UtcNow.AddMinutes(1));
        }
    }
}
