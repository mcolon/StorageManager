using System;

namespace StorageManager.Exceptions
{
    public class StorageNotImplementedException : NotImplementedException
    {
        public StorageNotImplementedException(string message) : base(message)
        {
        }
    }
}