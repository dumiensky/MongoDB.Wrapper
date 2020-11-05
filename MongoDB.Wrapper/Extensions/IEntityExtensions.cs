using MongoDB.Wrapper.Abstractions;

namespace MongoDB.Wrapper.Extensions
{
    public static class IEntityExtensions
    {
        public static bool IsNullOrDeleted(this IEntity entity)
        {
            return entity?.Deleted ?? true;
        }
    }
}
