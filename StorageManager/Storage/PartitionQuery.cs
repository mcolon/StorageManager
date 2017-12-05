using System.Collections.Generic;
using System.Linq.Expressions;

namespace StorageManager.Storage
{
    internal class PartitionQuery
    {
        public string PartitionName { get; set; }
        public string CompositePartitionValue { get; set; }
        public List<string> PartitionMatchsValues { get; }

        public List<Expression> Filters = new List<Expression>();

        public PartitionQuery()
        {
            PartitionMatchsValues = new List<string>();
        }
    }
}