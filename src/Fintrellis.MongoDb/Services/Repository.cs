using Fintrellis.MongoDb.Extensions;
using Fintrellis.MongoDb.Interfaces;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Fintrellis.MongoDb.Services
{
    public class Repository<T>(IMongoDatabase database): IRepository<T> where T : class, IMongoEntity
    {
        private readonly IMongoCollection<T> _collection = database.GetCollection<T>(MongoEntityExtensions.GetCollectionName<T>())!;

        /// <inheritdoc/>
        public async Task InsertOneAsync(T data)
        {
            await _collection.InsertOneAsync(data);
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var findAsync = await _collection.FindAsync(GetDefaultPredicateIfNull(predicate));

            var entities =
             await findAsync
                 .ToListAsync();

            return [.. entities];
        }

        /// <inheritdoc/>
        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null)
        {
            // could be better with aggregate but thats a long route
            var result = await GetAllAsync(GetDefaultPredicateIfNull(predicate));
            return result?.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task DeleteOneAsync(Expression<Func<T, bool>> predicate)
        {
            await _collection.DeleteOneAsync(predicate);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedDateTime = DateTime.UtcNow;
            await _collection!.FindOneAndReplaceAsync(Builders<T>.Filter.Eq(p => p.Id, entity.Id), entity);
        }

        private static Expression<Func<T, bool>> GetDefaultPredicateIfNull(Expression<Func<T, bool>>? predicate) => predicate ?? (x => true);
    }
}
