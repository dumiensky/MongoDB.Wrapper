using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MongoDB.Wrapper.Abstractions
{
    public interface IEntity
    {
        /// <summary>
        /// Id of an entity
        /// </summary>
        [BsonId] // This is the key of the entity
        Guid Id { get; set; }

        /// <summary>
        /// When the entity was added
        /// </summary>
        [BsonRepresentation(BsonType.String)] // To properly support linq queries. See: https://stackoverflow.com/q/16765543
        DateTimeOffset Added { get; set; }

        /// <summary>
        /// Deleted state flag (soft deletion)
        /// </summary>
        bool Deleted { get; set; }
    }
}
