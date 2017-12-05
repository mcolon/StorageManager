using System;

namespace StorageManager.Exceptions
{
    public class StorageNotFoundException : ArgumentException
    {
        public StorageNotFoundException(string message) : base(message)
        {
        }
    }
}