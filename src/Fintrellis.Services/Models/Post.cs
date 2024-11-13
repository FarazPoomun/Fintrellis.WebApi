using Fintrellis.MongoDb.Attributes;
using Fintrellis.MongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fintrellis.Services.Models
{
    [CollectionName("Posts")]
    public class Post:  Entity
    {
        [BsonRepresentation(BsonType.String)]
        public Guid PostId { get; set; } 
        public string? Title { get; set; } 
        public string? Content { get; set; } 
        public DateTime PublishedDate { get; set; } 
        public string? Author { get; set; }
    }
}