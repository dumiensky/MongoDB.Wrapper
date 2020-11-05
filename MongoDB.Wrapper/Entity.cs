using MongoDB.Wrapper.Abstractions;
using System;

namespace MongoDB.Wrapper
{
    public abstract class Entity : IEntity
    {
        public Guid Id { get; set; }

        public DateTimeOffset Added { get; set; }

        public bool Deleted { get; set; }
    }
}
