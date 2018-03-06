using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using StorageManager.Helpers;
using StorageManager.Interfaces;

namespace StorageManager.Query
{
    public abstract class StorageQueryable : IStorageQueryable
    {
        protected StorageQueryable(StorageQueryProvider provider, Type type, Expression expression = null)
        {
            StorageProvider = provider;
            Expression = expression ?? Expression.Constant(this);
            ElementType = type;
            HasMoreResult = true;
        }
        internal string Context;
        public bool HasMoreResult { get; internal set; }
        public Expression Expression { get; }
        public Type ElementType { get; }
        internal StorageQueryProvider StorageProvider { get; }
        public IQueryProvider Provider => StorageProvider;
        IEnumerator IEnumerable.GetEnumerator()
        {
            var result = (IEnumerable)Provider.Execute(Expression);
            return result.GetEnumerator();
        }

        async Task<IEnumerable> IStorageQueryable.Populate()
        {
            return (IEnumerable) await StorageProvider.ExecuteAsync(Expression).ConfigureAwait(false);
        }

        async Task<StorageQueryResult> IStorageQueryable.PopulatePaged(string token)
        {
            return (StorageQueryResult) await StorageProvider.ExecuteAsync(token).ConfigureAwait(false);
        }
    }

    public class StorageQueryable<T> : StorageQueryable, IStorageQueryable<T>
    {
        public StorageQueryable(StorageQueryProvider provider, Expression expression = null) : base(provider, typeof(T), expression)
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            var result = string.IsNullOrWhiteSpace(Context)
                ? (StorageQueryResult<T>) AsyncHelpers.RunSync(() => StorageProvider.ExecuteAsync(Expression)) 
                : (StorageQueryResult<T>) AsyncHelpers.RunSync(() => StorageProvider.ExecuteAsync(Context));
            Context = result.Contexts;
            HasMoreResult = result.HasMoreResult;
            return result.Records.GetEnumerator();
        }

        public async Task<IEnumerable<T>> Populate()
        {
            var result = string.IsNullOrWhiteSpace(Context)
                ? (StorageQueryResult<T>) await StorageProvider.ExecuteAsync(Expression).ConfigureAwait(false)
                : (StorageQueryResult<T>) await StorageProvider.ExecuteAsync(Context).ConfigureAwait(false);
            Context = result.Contexts;
            HasMoreResult = result.HasMoreResult;
            return result.Records;
        }

        public async Task<StorageQueryResult<T>> PopulatePaged(string context)
        {
            var result = string.IsNullOrWhiteSpace(context) 
                ? (StorageQueryResult<T>)await StorageProvider.ExecuteAsync(Expression).ConfigureAwait(false)
                : (StorageQueryResult<T>)await StorageProvider.ExecuteAsync(Context).ConfigureAwait(false);
            Context = result.Contexts;
            HasMoreResult = result.HasMoreResult;
            return result;
        }
    }
}