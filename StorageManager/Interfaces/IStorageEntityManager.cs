using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StorageManager.Query;

namespace StorageManager.Interfaces
{
    public interface IStorageEntityManager<T> : IStorageQueryable<T>
    {
        event Func<IStorageEntityManager<T>, T, Task<bool>> Inserting;
        event Func<IStorageEntityManager<T>, T, Task> Inserted;
        event Func<IStorageEntityManager<T>, T, Task<bool>> Updating;
        event Func<IStorageEntityManager<T>, T, Task> Updated;
        event Func<IStorageEntityManager<T>, T, Task<bool>> Deleting;
        event Func<IStorageEntityManager<T>, T, Task> Deleted;

        Task Insert(T entity);
        Task Insert(IEnumerable<T> entities);
        Task Update(T entity);
        Task Update(IEnumerable<T> entities);
        Task InsertOrReplaceUpsert(T entity);
        Task InsertOrReplaceUpsert(IEnumerable<T> entities);
        Task Delete(T entity);
        Task Delete(IEnumerable<T> entities);

        IEnumerable<object> GetIdValues(T entity);
        Task<StorageQueryResult<T>> NextPageAsync(string token);
    }
}