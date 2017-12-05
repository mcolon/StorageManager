using StorageManager.Query;

namespace StorageManager.Storage
{
    public class StorageFieldDefinition
    {
        public string Name { get; set; }
        public StorageQueryEntityMember Accesor { get; set; }
    }
}