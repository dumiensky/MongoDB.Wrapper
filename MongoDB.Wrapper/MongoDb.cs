using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Wrapper.Abstractions;
using MongoDB.Wrapper.Models;
using MongoDB.Wrapper.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Wrapper
{
    public class MongoDb : IMongoDb
    {
        readonly IMongoDatabase _database;

        public MongoDb(MongoDbSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _database =
                new MongoClient(settings.ConnectionString)
                .GetDatabase(settings.DatabaseName);
        }

        private IMongoCollection<TEntity> GetCollection<TEntity>() where TEntity : IEntity
        {
            return _database.GetCollection<TEntity>(typeof(TEntity).Name);
        }

        private IMongoCollection<KeyValueEntity> GetKeysCollection()
		{
            return _database.GetCollection<KeyValueEntity>("Keys");
		}

        public IMongoQueryable<TEntity> Query<TEntity>(bool includeDeleted = false) where TEntity : IEntity
        {
            IMongoQueryable<TEntity> query = GetCollection<TEntity>().AsQueryable();

            if (!includeDeleted)
                query = query.Where(_ => !_.Deleted);

            return query;
        }

        public Task Add<T>(T entity) where T : IEntity
        {
            entity.Id = Guid.NewGuid();
            entity.Added = DateTimeOffset.Now;

            return GetCollection<T>().InsertOneAsync(entity);
        }

        public Task<bool> Any<T>(Expression<Func<T, bool>> predicate, bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).AnyAsync(predicate);
        }

        public Task<bool> Any<T>(bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).AnyAsync();
        }

        public Task<int> Count<T>(Expression<Func<T, bool>> predicate, bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).CountAsync(predicate);
        }

        public Task<int> Count<T>(bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).CountAsync();
        }

        public Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> predicate, bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).FirstOrDefaultAsync(predicate);
        }

        public Task<T> SingleOrDefault<T>(Expression<Func<T, bool>> predicate, bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).SingleOrDefaultAsync(predicate);
        }

        public Task<T> Get<T>(Guid id) where T : IEntity
        {
            return FirstOrDefault<T>(_ => _.Id == id);
        }

        public Task<List<T>> Get<T>(Expression<Func<T, bool>> predicate, bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).Where(predicate).ToListAsync();
        }

        public Task<List<T>> Get<T>(bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).ToListAsync();
        }

        public async Task<bool> Replace<T>(T entity) where T : IEntity
        {
            if (entity.Id == default)
            {
                await Add(entity);
                return true;
            }

            if (await Get<T>(entity.Id) is T dbEntity)
            {
                entity.Deleted = dbEntity.Deleted;
                entity.Added = dbEntity.Added;

                return (await GetCollection<T>().ReplaceOneAsync(_ => _.Id == entity.Id, entity)).IsAcknowledged;
            }
            else
                throw new Exception($"Could not replace entity of type {typeof(T).Name}! Entity with id {entity.Id} does not exist!");
        }

        public Task<bool> Delete<T>(Guid id) where T : IEntity
        {
            return DeleteRestoreInternal<T>(id, true);
        }

        public async Task<long> DeleteMany<T>(Expression<Func<T, bool>> selector) where T : IEntity
		{
            var entities = await Get(selector);
            var results = new List<bool>();

			for (int i = 0; i < entities.Count; i += 50)
			{
                var toProcess = entities.Skip(i).Take(50);

                results.AddRange(
                    await Task.WhenAll(toProcess.Select(_ => DeleteRestoreInternal<T>(_.Id, true))));
			}

            return results.Count(_ => _);
		}

        public Task<bool> Restore<T>(Guid id) where T : IEntity
        {
            return DeleteRestoreInternal<T>(id, false);
        }

        private async Task<bool> DeleteRestoreInternal<T>(Guid id, bool state) where T : IEntity
        {
            var entity = await Get<T>(id);
            if (entity == null)
                throw new Exception($"Entity of type {typeof(T).Name} with id {id} was not found!");

            entity.Deleted = state;
            return (await GetCollection<T>().ReplaceOneAsync(_ => _.Id == entity.Id, entity)).IsAcknowledged;
        }

        public async Task<bool> DeleteHard<T>(Guid id) where T : IEntity
        {
            return (await GetCollection<T>().DeleteOneAsync(_ => _.Id == id)).IsAcknowledged;
        }

        public async Task<long> DeleteHardMany<T>(Expression<Func<T, bool>> selector) where T : IEntity
		{
            return (await GetCollection<T>().DeleteManyAsync(selector)).DeletedCount;
		}

        private Task<KeyValueEntity> GetKey(string key)
        {
            return GetKeysCollection().AsQueryable().FirstOrDefaultAsync(_ => _.Key == key);
        }

        public async Task SetKeyValue<T>(string key, T value)
		{
            if (await GetKeysCollection().AsQueryable().AnyAsync(_ => _.Key == key))
			{
                if (value.Equals(default(T)))
                    await GetKeysCollection().DeleteOneAsync(_ => _.Key == key);
                else
                    await GetKeysCollection().UpdateOneAsync(_ => _.Key == key, Builders<KeyValueEntity>.Update.Set(_ => _.Value, JsonConvert.SerializeObject(value)));
            }
            else
                await GetKeysCollection().InsertOneAsync(new KeyValueEntity { Key = key, Value = JsonConvert.SerializeObject(value) });
        }

        public async Task<T> GetKeyValue<T>(string key)
		{
            return (await GetKey(key))?.Value is string value
                ? JsonConvert.DeserializeObject<T>(value)
                : default;
		}
    }
}
