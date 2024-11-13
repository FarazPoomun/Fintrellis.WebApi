using Fintrellis.MongoDb.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fintrellis.MongoDb.Models
{
    public class Entity : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id", Order = 1)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("_createdDateTime", Order = 2)]
        [BsonRepresentation(BsonType.String)]
        public DateTime CreatedDateTime { get; set; }

        [BsonElement("_updatedDateTime", Order = 3)]
        [BsonRepresentation(BsonType.String)]
        public DateTime? UpdatedDateTime { get; set; }
    }
}
