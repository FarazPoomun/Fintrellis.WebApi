using Fintrellis.MongoDb.Attributes;
using Fintrellis.MongoDb.Interfaces;

namespace Fintrellis.MongoDb.Extensions
{
    public class EntityExtensions
    {
        public static string GetCollectionName<T>() where T : class, IEntity
        {
            var collectionNameAttribute = (CollectionNameAttribute)typeof(T).GetCustomAttributes(typeof(CollectionNameAttribute), true).FirstOrDefault();
            return collectionNameAttribute != null ? collectionNameAttribute.Name.ToLower() : typeof(T).Name.ToLower();
        }
    }
}
