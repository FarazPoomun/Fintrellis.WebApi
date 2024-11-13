namespace Fintrellis.MongoDb.Interfaces
{
    /// <summary>
    /// Interface: Mongo Entity
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the Unique Identifier
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Updated Date
        /// </summary>
        DateTime? UpdatedDateTime { get; set; }
    }
}