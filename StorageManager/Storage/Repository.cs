using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using StorageManager.Interfaces;
using StorageManager.Query;

namespace StorageManager.Storage
{
    public class Repository<T> : IRepository<T>
    {
        protected readonly IStorageEntityManager<T> Storage;

        public Repository(IStorageEntityManager<T> promoStorage)
        {
            Storage = promoStorage;
        }

        public virtual async Task<T> Insert(T entity)
        {
            await Storage.Insert(entity).ConfigureAwait(false);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> Insert(IEnumerable<T> entities)
        {
            var toInsert = entities as T[] ?? entities.ToArray();
            List<T> result = new List<T>();
            foreach (var entity in toInsert)
                result.Add(await Insert(entity).ConfigureAwait(false));
            return result;
        }

        public virtual async Task<T> Update(T entity)
        {
            await Storage.Update(entity).ConfigureAwait(false);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> Update(IEnumerable<T> entities)
        {
            var toUpdate = entities as T[] ?? entities.ToArray();
            List<T> result = new List<T>();
            foreach (var entity in toUpdate)
                result.Add(await Update(entity).ConfigureAwait(false));
            return result;
        }

        public virtual async Task<T> Upsert(T entity)
        {
            await Storage.Upsert(entity).ConfigureAwait(false);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> Upsert(IEnumerable<T> entities)
        {
            var toUpsert = entities as T[] ?? entities.ToArray();
            List<T> result = new List<T>();
            foreach (var entity in toUpsert)
                result.Add(await Upsert(entity).ConfigureAwait(false));
            return result;
        }

        public virtual async Task<T> Delete(T entity)
        {
            await Storage.Delete(entity).ConfigureAwait(false);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> Delete(IEnumerable<T> entities)
        {
            var toDelete = entities as T[] ?? entities.ToArray();
            List<T> result = new List<T>();
            foreach (var entity in toDelete)
                result.Add(await Delete(entity).ConfigureAwait(false));
            return result;
        }

        public IEnumerable<object> GetIdValues(T entity)
        {
            return Storage.GetIdValues(entity);
        }

        public virtual bool HasMoreResult => Storage.HasMoreResult;

        public Task<StorageQueryResult<T>> NextPageAsync(string token)
        {
            return Storage.NextPageAsync(token);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Storage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Storage.GetEnumerator();
        }

        public Expression Expression => Storage.Expression;
        public Type ElementType => Storage.ElementType;
        public IQueryProvider Provider => Storage.Provider;

        public async Task<IEnumerable<T>> Populate()
        {
            return await Storage.Populate().ConfigureAwait(false);
        }

        async Task<IEnumerable> IStorageQueryable.Populate()
        {
            return await Storage.Populate().ConfigureAwait(false);
        }

        public async Task<StorageQueryResult<T>> PopulatePaged(string token)
        {
            return await Storage.PopulatePaged(token).ConfigureAwait(false);
        }

        async Task<StorageQueryResult> IStorageQueryable.PopulatePaged(string token)
        {
            return await Storage.PopulatePaged(token).ConfigureAwait(false);
        }
    }
}