using System;

namespace StorageManager.Exceptions
{
    public class StorageInvalidOperationException : InvalidOperationException
    {
        public StorageInvalidOperationException(string message) : base(message)
        {
        }
    }
}