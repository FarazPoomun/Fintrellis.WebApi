using Fintrellis.MongoDb.Interfaces;
using System.Linq.Expressions;

namespace Fintrellis.Services.Interfaces
{
    /// <summary>
    /// A proxy for the repository that also communicates with redis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICachedRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Deletes one from cache and repository
        /// </summary>
        /// <param name="cacheId"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task DeleteOneAsync(string cacheId, Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets all from db, if cacheId provided, gets from cache if exists
        /// </summary>
        /// <param name="cacheId"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<List<T>> GetAllAsync(string? cacheId = null, Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Get one from entity or repository
        /// </summary>
        /// <param name="cacheId"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<T?> GetFirstOrDefaultAsync(string cacheId, Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Inserts into repository and cache
        /// </summary>
        /// <param name="cacheId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task InsertOneAsync(string cacheId, T entity);

        /// <summary>
        /// Updates the repository and cache
        /// </summary>
        /// <param name="cacheId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync(string cacheId, T entity);
    }
}