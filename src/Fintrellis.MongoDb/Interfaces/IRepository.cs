using System.Linq.Expressions;

namespace Fintrellis.MongoDb.Interfaces
{
    public interface IRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Inserts entity in the collection
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task InsertOneAsync(T entity);

        /// <summary>
        /// Get a list of entity filtered by the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Get first or null filtered by the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Deletes one entity based on entity
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task DeleteOneAsync(Expression<Func<T, bool>> predicate = null);

        /// <summary>
        /// Updates entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync(T entity);
    }
}
