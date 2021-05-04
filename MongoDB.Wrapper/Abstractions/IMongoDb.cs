using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDB.Wrapper.Abstractions
{
    public interface IMongoDb
    {
        /// <summary>
        /// Returns a query to operate on
        /// </summary>
        /// <typeparam name="TEntity">Type of entities</typeparam>
        /// <param name="includeDeleted">When true, items returned in evaluation of query will contain (soft) deleted entites, otherwise they will be filtered out</param>
        IMongoQueryable<TEntity> Query<TEntity>(bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Assigns a new Id to entity and adds it to collection of type TEntity
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="entity">Entity to add</param>
        Task Add<TEntity>(TEntity entity) where TEntity : IEntity;

        /// <summary>
        /// Returns whether the collection of type TEntity contains entites matching the predicate
        /// </summary>
        /// <typeparam name="TEntity">Type of entities</typeparam>
        /// <param name="predicate">Predicate to compare entities against</param>
        /// <param name="includeDeleted">When set to true, the query will also look for (soft) deleted entites</param>
        Task<bool> Any<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Returns whether the collection of type TEntity contains entites
        /// </summary>
        /// <typeparam name="TEntity">Type of entities</typeparam>
        /// <param name="includeDeleted">When set to true, the query will also look for (soft) deleted entites</param>
        Task<bool> Any<TEntity>(bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Returns the count of entites in the collection of type TEntity that match given predicate
        /// </summary>
        /// <typeparam name="TEntity">Type of entities</typeparam>
        /// <param name="predicate">Predicate to compare entities against</param>
        /// <param name="includeDeleted">When set to true, the query will also count (soft) deleted entites</param>
        Task<int> Count<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Returns the count of entites in the collection of type TEntity
        /// </summary>
        /// <typeparam name="TEntity">Type of entities</typeparam>
        /// <param name="includeDeleted">When set to true, the query will also count (soft) deleted entites</param>
        Task<int> Count<TEntity>(bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Returns entity of type TEntity, which Id equals passed Guid. Default if not found
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="id">Entity Id</param>
        Task<TEntity> Get<TEntity>(Guid id) where TEntity : IEntity;

        /// <summary>
        /// Returns the first entity from collection of type TEntity, that matches the predicate
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="predicate">Predicate to compare entities against</param>
        /// <param name="includeDeleted">When set to true, the query will also search within (soft) deleted entites</param>
        Task<TEntity> FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Returns the entity from collection of type TEntity, that matches the predicate. If more than one entity matches the predicate, an exception is thrown
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="predicate">Predicate to compare entities against</param>
        /// <param name="includeDeleted">When set to true, the query will also search within (soft) deleted entites</param>
        Task<TEntity> SingleOrDefault<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Returns list of entities from collection of type TEntity, that match the predicate
        /// </summary>
        /// <typeparam name="TEntity">Type of entities</typeparam>
        /// <param name="predicate">Predicate to compare entities against</param>
        /// <param name="includeDeleted">When set to true, the query will also return (soft) deleted entites</param>
        Task<List<TEntity>> Get<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Returns list of entities from collection of type TEntity
        /// </summary>
        /// <typeparam name="TEntity">Type of entities</typeparam>
        /// <param name="includeDeleted">When set to true, the query will also return (soft) deleted entites</param>
        Task<List<TEntity>> Get<TEntity>(bool includeDeleted = false) where TEntity : IEntity;

        /// <summary>
        /// Replaces the object of type TEntity with passed object, compared by IEntity.Id. If IEntity.Id equals default, entity is added.
        /// When entity with given id is not found, an exception is thrown.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="entity">Updated (or new) entity</param>
        Task<bool> Replace<TEntity>(TEntity entity) where TEntity : IEntity;

        /// <summary>
        /// Soft deletes (sets Deleted flag) on entity with given id. Throws an exception if the entity doesn't exist.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="id">Id of the entity</param>
        Task<bool> Delete<TEntity>(Guid id) where TEntity : IEntity;

        /// <summary>
        /// Soft deletes (sets Deleted flag) the entities matching the selector.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities</typeparam>
        /// <param name="selector">Entity selector</param>
        /// <returns>Number of entities deleted</returns>
        Task<long> DeleteMany<T>(Expression<Func<T, bool>> selector) where T : IEntity;

        /// <summary>
        /// Reverts a soft delete (un-sets Deleted flag) of an entity with given id. Throws an exception if the entity doesn't exist.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="id">Id of the entity</param>
        Task<bool> Restore<TEntity>(Guid id) where TEntity : IEntity;

        /// <summary>
        /// Deleted an entity with given id from the database. This is irreversible. Throws an exception if the entity doesn't exist.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="id">Id of the entity</param>
        Task<bool> DeleteHard<TEntity>(Guid id) where TEntity : IEntity;

        /// <summary>
        /// Deletes all entites matching the selector from the database. This is irreversible.
        /// </summary>
        /// <typeparam name="T">Type of entities</typeparam>
        /// <param name="selector">Entity selector</param>
        /// <returns>Number of entities deleted</returns>
        Task<long> DeleteHardMany<T>(Expression<Func<T, bool>> selector) where T : IEntity;

        /// <summary>
        /// Persists the value to the key in the database
        /// </summary>
		Task SetKeyValue<T>(string key, T value);

        /// <summary>
        /// Gets the value associated to the key
        /// </summary>
        /// <typeparam name="T">Type of value to retrieve</typeparam>
        /// <returns>Value or default</returns>
		Task<T> GetKeyValue<T>(string key);
	}
}
