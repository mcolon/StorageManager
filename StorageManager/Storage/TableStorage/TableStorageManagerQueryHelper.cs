using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;
using StorageManager.Extensions;
using StorageManager.Query;

namespace StorageManager.Storage.TableStorage
{
    internal class TableStorageManagerQueryHelper<T>
    {
        internal readonly List<IEnumerable<Expression>> Expressions = new List<IEnumerable<Expression>>();
        internal readonly StorageEntityDefinition<T> EntityDefinition;

        public TableStorageManagerQueryHelper(StorageEntityDefinition<T> definition)
        {
            EntityDefinition = definition;
        }
    }
}