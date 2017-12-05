using System;

namespace StorageManager.Exceptions
{
    public class StorageNotSupportedException : NotSupportedException
    {
        public StorageNotSupportedException(string message) : base(message)
        {
        }
    }
}