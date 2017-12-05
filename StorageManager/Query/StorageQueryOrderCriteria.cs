using StorageManager.Enums;

namespace StorageManager.Query
{
    public class StorageQueryOrderCriteria
    {
        public StorageQueryEntityMember OrderField { get; set; }
        public QueryOrderDirection Direction { get; set; }
    }
}