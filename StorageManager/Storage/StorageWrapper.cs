using System.Collections.Generic;
using System.Threading.Tasks;
using StorageManager.Configuration;
using StorageManager.Query;
using System.Linq.Expressions;
using StorageManager.Enums;
using StorageManager.Exceptions;

namespace StorageManager.Storage
{
    public abstract class StorageWrapper
    {
        protected const int DEFAULT_PAGE_SIZE = 100;
    }


    public abstract class StorageWrapper<T> : StorageWrapper
    {

        protected readonly StorageEntityDefinition<T> EntityDefinition;
        protected IStorageConfiguration Configuration;

        protected StorageWrapper(StorageEntityDefinition<T> definition, IStorageConfiguration configuration)
        {
            EntityDefinition = definition;
            Configuration = configuration;
        }

        public abstract Task Insert(T entity);

        public abstract Task Update(T entity);

        public abstract Task Delete(T entity);

        public abstract Task InsertOrReplaceUpsertUpsert(T entity);


        public abstract Task Insert(IEnumerable<T> entity);

        public abstract Task Update(IEnumerable<T> entity);

        public abstract Task Delete(IEnumerable<T> entity);

        public abstract Task InsertOrReplaceUpsertUpsert(IEnumerable<T> entity);



        public abstract Task<StorageQueryResult<T>> ExecuteQueryAsync(Expression expression);
        public abstract Task<StorageQueryResult<T>> ExecuteQueryAsync(string context, int? pageSize = null);
    }
}