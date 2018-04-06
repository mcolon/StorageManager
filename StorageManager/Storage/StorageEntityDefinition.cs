using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using StorageManager.Enums;
using StorageManager.Exceptions;
using StorageManager.Extensions;
using StorageManager.Query;

namespace StorageManager.Storage
{
    public abstract class StorageEntityDefinition
    {
        protected List<StorageFieldDefinition> _filterableFields = new List<StorageFieldDefinition>();
        protected List<StorageQueryEntityMember> _idFields = new List<StorageQueryEntityMember>();
        protected List<StoragePartitionDefinition> _partitions = new List<StoragePartitionDefinition>();

        public IEnumerable<StoragePartitionDefinition>  Partitions()
        {
            var result = new List<StoragePartitionDefinition>();
            result.AddRange(_partitions);
            result.Add(new StoragePartitionDefinition
            {
                Name = null,
                Expressions = _idFields
            });
            return result;
        }

        public IEnumerable<StorageFieldDefinition> Filters()
        {
            return _filterableFields;
        }

        public bool AllowTableScan { get; set; }
        public bool AutoRebuildPartitionIfRequire { get; set; }
        public StorageType StorageType { get; set; }

        public abstract string TableName();
        public StorageEntityDefinition TableName(string name)
        {
            Name = name;
            return this;
        }

        protected string Name { get; set; }


    }

    public class StorageEntityDefinition<T> : StorageEntityDefinition
    {

        public StorageEntityDefinition<T> Filterable<TType>(string name=null, Expression<Func<T, TType>> filter=null)
        {
            if (filter == null)
                throw new StorageArgumentException("filter can not be null");

            name = name ?? ((MemberExpression)filter.Body).Member.Name;
            var newFilter = new StorageFieldDefinition { Name = name, Accesor = filter.ToMemberAccessInfo() };

            if (_filterableFields.Any(f => f.Name == newFilter.Name))
                throw new StorageArgumentException($"Already exist a field with name '{name}'");

            if (_filterableFields.Any(f => f.Accesor.MemberPath == newFilter.Accesor.MemberPath))
                throw new StorageArgumentException($"Already exist a field with signature '{newFilter.Accesor.MemberPath}'");

            _filterableFields.Add(new StorageFieldDefinition{ Name = name, Accesor = filter.ToMemberAccessInfo() });
            return this;
        }
        public StorageEntityDefinition<T> Id<TType>(Expression<Func<T, TType>> idFields)
        {

            if (idFields.Body.NodeType == ExpressionType.MemberAccess)
            {
                _idFields.Add(idFields.Body.ToMemberAccessInfo());
            }
            else if (idFields.Body.NodeType == ExpressionType.New)
            {
                var expressions = ((NewExpression) idFields.Body).Arguments.Select(arg => arg.ToMemberAccessInfo());
                _idFields.AddRange(expressions);
            }

            return this;
        }
        public StorageEntityDefinition<T> Partition<TType>(string indexName, Expression<Func<T, TType>> func)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new StorageArgumentException("Index Name Can not be null");

            if (_partitions.Any(p => p.Name == indexName))
                throw new StorageArgumentException($"Already exist the index name '{indexName}'");

            if (func == null)
                throw new StorageArgumentException($"Fields not spedified for index '{indexName}'");

            var indexExpressions = new List<StorageQueryEntityMember>();

            if (func.Body.NodeType == ExpressionType.MemberAccess)
                indexExpressions.Add(func.Body.ToMemberAccessInfo());
            else if (func.Body.NodeType == ExpressionType.New)
                indexExpressions.AddRange(((NewExpression)func.Body).Arguments.Select(arg => arg.ToMemberAccessInfo()));


            _partitions.Add(new StoragePartitionDefinition{Name = indexName, Expressions = indexExpressions});
            return this;
        }

        public IEnumerable<object> GetIdValues(T entity)
        {
            return _idFields.Select(f => f.Evaluate(entity));
        }

        public override string TableName()
        {
            return string.IsNullOrWhiteSpace(Name) ? typeof(T).Name : Name;
        }
    }
}