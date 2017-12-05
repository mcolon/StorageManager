using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using StorageManager.Exceptions;
using StorageManager.Interfaces;
using StorageManager.Query;
using StorageManager.Storage;

namespace StorageManager.Extensions
{
    public static class QueryableExtensions
    {
        public static bool HasMoreResult(this IQueryable source)
        {
            if (source is IStorageQueryable storageQuery)
            {
                return storageQuery.HasMoreResult;
            }

            if (source.GetType().GetGenericTypeDefinition() == typeof(StorageEntityManager<>))
            {
                PropertyInfo hasMore = source.GetType().GetProperty("HasMoreResult");
                if (hasMore != null)
                    return (bool) hasMore.GetValue(source);
            }
            throw new StorageNotImplementedException($"Type {source.GetType().Name} is not a implementation of {typeof(StorageQueryable)}");
        }


        public static async Task<IEnumerable> Populate(this IQueryable source)
        {


            if (source is IStorageQueryable storageQuery)
            {
                Type storageType = storageQuery.GetType().GetGenericArguments().First();
                var genericMethod = typeof(QueryableExtensions).GetMethods().FirstOrDefault(t => t.Name == "Populate" && t.GetGenericArguments().Any());
                var method = genericMethod.MakeGenericMethod(storageType);
                return await ((dynamic)method.Invoke(null, new object[] { source })).ConfigureAwait(false);
            }
            throw new StorageNotImplementedException($"Type {source.GetType().Name} is not a implementation of {typeof(StorageQueryable)}");
        }

        public static async Task<StorageQueryResult> PopulatePaged(this IQueryable source)
        {


            if (source is IStorageQueryable storageQuery)
            {
                Type storageType = storageQuery.GetType().GetGenericArguments().First();
                var genericMethod = typeof(QueryableExtensions).GetMethods().FirstOrDefault(t => t.Name == "PopulatePaged" && t.GetGenericArguments().Any());
                var resultType = typeof(StorageQueryResult<>).MakeGenericType(storageType);
                var method = genericMethod.MakeGenericMethod(resultType);
                return await ((dynamic)method.Invoke(null, new object[] { source })).ConfigureAwait(false);
            }
            throw new StorageNotImplementedException($"Type {source.GetType().Name} is not a implementation of {typeof(StorageQueryable)}");
        }



        public static async Task<List<T>> Populate<T>(this IQueryable source)
        {
            if (source is IStorageQueryable<T> storageQuery)
            {
                return (await storageQuery.Populate().ConfigureAwait(false)).ToList();
            }
            throw new StorageNotImplementedException($"Type {source.GetType().Name} is not a implementation of {typeof(StorageQueryable)}");
        }



        public static async Task<StorageQueryResult<T>> PopulatePaged<T>(this IQueryable source)
        {
            if (source is StorageQueryable<T> storageQuery)
            {
                return (StorageQueryResult<T>) await storageQuery.PopulatePaged(storageQuery.Context).ConfigureAwait(false);
            }
            throw new StorageNotImplementedException($"Type {source.GetType().Name} is not a implementation of {typeof(StorageQueryable)}");
        }
    }
}