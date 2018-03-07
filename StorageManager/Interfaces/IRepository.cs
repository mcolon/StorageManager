using System.Collections.Generic;
using System.Threading.Tasks;
using StorageManager.Query;

namespace StorageManager.Interfaces
{
    public interface IRepository<T> : IStorageQueryable<T>
    {
        Task<T> Insert(T entity);
        Task<IEnumerable<T>> Insert(IEnumerable<T> entities);
        Task<T> Update(T entity);
        Task<IEnumerable<T>> Update(IEnumerable<T> entities);
        Task<T> InsertOrReplaceUpsert(T entity);
        Task<IEnumerable<T>> InsertOrUpdate(IEnumerable<T> entities);
        Task<T> Delete(T entity);
        Task<IEnumerable<T>> Delete(IEnumerable<T> entities);
        IEnumerable<object> GetIdValues(T entity);
        Task<StorageQueryResult<T>> NextPageAsync(string token);
    }
}