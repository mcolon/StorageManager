using System.Collections.Generic;
using System.Linq;
using StorageManager.Query;

namespace StorageManager.Storage
{
    public class StoragePartitionDefinition
    {
        public string Name { get; set; }
        public List<StorageQueryEntityMember> Expressions { get; set; }
        public IEnumerable<string> ExpressionNames => Expressions?.Select(e => e.ToString());
    }
}