using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorageManager.Query;

namespace StorageManager.Interfaces
{
    public interface IStorageQueryable : IEnumerable
    {
        bool HasMoreResult { get; }
        Task<IEnumerable> Populate();
        Task<StorageQueryResult> PopulatePaged(string token);
    }

    public interface IStorageQueryable<T> : IStorageQueryable, IOrderedQueryable<T>
    {
        new Task<IEnumerable<T>> Populate();
        new Task<StorageQueryResult<T>> PopulatePaged(string token);
    }
}