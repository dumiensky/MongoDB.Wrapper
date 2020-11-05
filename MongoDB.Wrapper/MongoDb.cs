using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Wrapper.Abstractions;
using MongoDB.Wrapper.Settings;
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

        private IMongoCollection<TEntity> GetCollection<TEntity>() where TEntity: IEntity
        {
            return _database.GetCollection<TEntity>(typeof(TEntity).Name);
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
            return Query<T>().FirstOrDefaultAsync(_ => _.Id == id);
        }

        public Task<List<T>> Get<T>(Expression<Func<T, bool>> predicate, bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).Where(predicate).ToListAsync();
        }

        public Task<List<T>> Get<T>(bool includeDeleted = false) where T : IEntity
        {
            return Query<T>(includeDeleted).ToListAsync();
        }

        public async Task Replace<T>(T entity) where T : IEntity
        {
            if (entity.Id == default)
            {
                await Add(entity);
                return;
            }

            Expression<Func<T, bool>> sameId = _ => _.Id == entity.Id;

            if (await Any(sameId))
                await GetCollection<T>().ReplaceOneAsync(sameId, entity);
            else
                throw new Exception($"Could not replace entity of type {typeof(T).Name}! Entity with id {entity.Id} does not exist!");
        }
    }
}
