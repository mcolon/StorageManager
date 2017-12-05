using System;

namespace StorageManager.Exceptions
{
    public class StorageArgumentNullException : ArgumentNullException
    {
        public StorageArgumentNullException(string message) : base(message)
        {
        }

        public StorageArgumentNullException(string paramName, string message) : base(paramName, message)
        {
        }
    }
}