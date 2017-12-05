using System;

namespace StorageManager.Exceptions
{
    public class StorageArgumentException : ArgumentException
    {
        public StorageArgumentException(string message) : base(message)
        {
        }
    }
}
