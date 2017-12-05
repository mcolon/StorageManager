using System.Configuration;

namespace StorageManager.Configuration
{
    public class StorageConfiguration : IStorageConfiguration
    {
        public string StorageAccount => ConfigurationManager.ConnectionStrings["storageConnection"].ConnectionString;
    }
}
