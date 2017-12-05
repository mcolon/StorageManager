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

        public StorageEntityManager<T> Provider { get; }

        protected StorageWrapper(StorageEntityDefinition<T> definition, IStorageConfiguration configuration, StorageEntityManager<T> provider)
        {
            EntityDefinition = definition;
            Configuration = configuration;
            Provider = provider;
        }

        public abstract Task Insert(T entity);

        public abstract Task Update(T entity);

        public abstract Task Delete(T entity);

        public abstract Task Upsert(T entity);


        public abstract Task Insert(IEnumerable<T> entity);

        public abstract Task Update(IEnumerable<T> entity);

        public abstract Task Delete(IEnumerable<T> entity);

        public abstract Task Upsert(IEnumerable<T> entity);



        public abstract Task<StorageQueryResult<T>> ExecuteQueryAsync(Expression expression);
        public abstract Task<StorageQueryResult<T>> ExecuteQueryAsync(string context, int? pageSize = null);
        public abstract StorageQueryResult<T> ExecuteQuery(Expression expression);
        public abstract StorageQueryResult<T> ExecuteQuery(string context, int? pageSize = null);
    }
}