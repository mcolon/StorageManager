using System;

namespace StorageManager.Exceptions
{
    public class StorageArgumentOutOfRangeException : ArgumentOutOfRangeException
    {
        public StorageArgumentOutOfRangeException(string param, string message) : base(param, message)
        {
        }
    }
}