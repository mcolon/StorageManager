using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using StorageManager.Helpers;
using StorageManager.Interfaces;

namespace StorageManager.Query
{
    public abstract class StorageQueryProvider : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystemHelper.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(StorageQueryable<>).MakeGenericType(elementType), this, expression);
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                    throw tie.InnerException;

                throw;
            }
        }

        public IQueryable<TK> CreateQuery<TK>(Expression expression)
        {
            return new StorageQueryable<TK>(this, expression);
        }

        public abstract object Execute(Expression expression);

        public abstract object Execute(string queryContext);

        public abstract TResult Execute<TResult>(Expression expression);


        public abstract Task<object> ExecuteAsync(Expression expression);

        public abstract Task<object> ExecuteAsync(string queryContext);

        public abstract Task<TResult> ExecuteAsync<TResult>(Expression expression);

    }


}