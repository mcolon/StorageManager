using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using StorageManager.Configuration;
using StorageManager.Helpers;
using StorageManager.Interfaces;
using StorageManager.Query;

namespace StorageManager.Storage
{
    public class StorageEntityManager<T> : StorageQueryProvider, IStorageEntityManager<T>
    {
        internal readonly StorageQueryable<T> Queryable;
        internal readonly IStorageConfiguration Configuration;
        internal readonly StorageEntityDefinition<T> EntityDefinition;
        internal readonly StorageWrapper<T> Wrapper;

        public event Func<IStorageEntityManager<T>, T, Task<bool>> Inserting;
        public event Func<IStorageEntityManager<T>, T, Task> Inserted;
        public event Func<IStorageEntityManager<T>, T, Task<bool>> Updating;
        public event Func<IStorageEntityManager<T>, T, Task> Updated;
        public event Func<IStorageEntityManager<T>, T, Task<bool>> Deleting;
        public event Func<IStorageEntityManager<T>, T, Task> Deleted;

        public StorageEntityManager(StorageEntityDefinition<T> definition, IStorageConfiguration configuration, StorageWrapper<T> wrapper)
        {
            EntityDefinition = definition;
            Wrapper = wrapper;
            Queryable = new StorageQueryable<T>(Wrapper.Provider);
            Configuration = configuration;
        }

        public async Task Insert(T entity)
        {
            bool inserting = await OnInserting(entity).ConfigureAwait(false);
            if (inserting)
            {
                await Wrapper.Insert(entity).ConfigureAwait(false);
                return;
            }
            await OnInserted(entity).ConfigureAwait(false);
        }

        public async Task Insert(IEnumerable<T> entities)
        {
            IEnumerable<Task> tasks = entities.Select(async e => await Insert(e));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task Upsert(T entity)
        {
            await Wrapper.Upsert(entity).ConfigureAwait(false);
        }

        public async Task Upsert(IEnumerable<T> entities)
        {
            IEnumerable<Task> tasks = entities.Select(async e => await Upsert(e));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task Update(T entity)
        {
            bool updating = await OnUpdating(entity).ConfigureAwait(false);
            if (updating)
            {
                await Wrapper.Update(entity).ConfigureAwait(false);
                return;
            }
            await OnUpdated(entity).ConfigureAwait(false);
        }

        public async Task Update(IEnumerable<T> entities)
        {
            IEnumerable<Task> tasks = entities.Select(async e => await Update(e).ConfigureAwait(false));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task Delete(T entity)
        {
            bool deleting = await OnDeleting(entity).ConfigureAwait(false);
            if (deleting)
            {
                await Wrapper.Delete(entity).ConfigureAwait(false);
                return;
            }
            await OnDeleted(entity).ConfigureAwait(false);
        }

        public async Task Delete(IEnumerable<T> entities)
        {
            IEnumerable<Task> tasks = entities.Select(async e => await Delete(e).ConfigureAwait(false));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public bool HasMoreResult => Queryable.HasMoreResult;

        public IEnumerable<object> GetIdValues(T entity)
        {
            return EntityDefinition.GetIdValues(entity);
        }

        public Task<StorageQueryResult<T>> NextPageAsync(string token)
        {
            return Wrapper.ExecuteQueryAsync(token);
        }


        public override object Execute(Expression expression)
        {
            return AsyncHelpers.RunSync(() => ExecuteAsync(expression));

        }

        public override TResult Execute<TResult>(Expression expression)
        {
            return AsyncHelpers.RunSync(() => ExecuteAsync<TResult>(expression));
        }

        public override async Task<object> ExecuteAsync(Expression expression)
        {
            return await Wrapper.ExecuteQueryAsync(expression).ConfigureAwait(false);
        }

        public override async Task<object> ExecuteAsync(string queryContext)
        {
            return string.IsNullOrWhiteSpace(queryContext)
                ? await Wrapper.ExecuteQueryAsync(Expression).ConfigureAwait(false)
                : await Wrapper.ExecuteQueryAsync(queryContext).ConfigureAwait(false);
        }

        public override async Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            var result = await ExecuteAsync(expression).ConfigureAwait(false);
            if (typeof(TResult) == typeof(T))
            {
                if (result is StorageQueryResult<T> r)
                {
                    var record = r.Records.FirstOrDefault();
                    return record == null ? default(TResult) : (TResult)Convert.ChangeType(record, typeof(TResult));
                }
            }
            return (TResult)result;
        }



        private async Task<bool> OnInserting(T args)
        {
            return await ProcessPreActions(Inserting, args).ConfigureAwait(false);
        }
        private async Task OnInserted(T args)
        {
            await ProcessPostActions(Inserted, args).ConfigureAwait(false);
        }
        private async Task<bool> OnUpdating(T args)
        {
            return await ProcessPreActions(Updating, args).ConfigureAwait(false);
        }
        private async Task OnUpdated(T args)
        {
            await ProcessPostActions(Updated, args).ConfigureAwait(false);
        }

        private async Task<bool> OnDeleting(T args)
        {
            return await ProcessPreActions(Deleting, args).ConfigureAwait(false);
        }
        private async Task OnDeleted(T args)
        {
            await ProcessPostActions(Deleted, args).ConfigureAwait(false);
        }


        private async Task<bool> ProcessPreActions(Func<StorageEntityManager<T>, T, Task<bool>> handler, T arg)
        {
            IEnumerable<Task<bool>> tasks = handler?.GetInvocationList()
                .Select(i => ((Func<StorageEntityManager<T>, T, Task<bool>>)i)(this, arg));

            if (tasks == null)
                return true;

            foreach (Task<bool> t in tasks)
            {
                var result = await t.ConfigureAwait(false);
                if (!result)
                    return false;
            }
            return true;
        }
        private async Task ProcessPostActions(Func<StorageEntityManager<T>, T, Task> handler, T arg)
        {
            var tasks = handler?.GetInvocationList()
                .Select(i => ((Func<StorageEntityManager<T>, T, Task>)i)(this, arg));

            if (tasks == null)
                return;

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #region IOrderedQueryable metods 

        public Expression Expression => Queryable.Expression;
        public Type ElementType => Queryable.ElementType;
        public IQueryProvider Provider => Queryable.Provider;

        public IEnumerator<T> GetEnumerator()
        {
            var result = Queryable.GetEnumerator();
            return result;
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            var result = GetEnumerator();
            return result;
        }

        #endregion


        public async Task<IEnumerable<T>> Populate()
        {
            return await Queryable.Populate().ConfigureAwait(false);
        }

        async Task<IEnumerable> IStorageQueryable.Populate()
        {
            return await Queryable.Populate().ConfigureAwait(false);
        }

        async Task<StorageQueryResult<T>> IStorageQueryable<T>.PopulatePaged(string token)
        {
            return await Queryable.PopulatePaged(token).ConfigureAwait(false);
        }

        public async Task<StorageQueryResult> PopulatePaged(string token)
        {
            return await Queryable.PopulatePaged(token).ConfigureAwait(false);
        }
    }
}