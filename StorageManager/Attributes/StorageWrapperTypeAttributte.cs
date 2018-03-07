using System;
using StorageManager.Enums;

namespace StorageManager.Attributes
{
    public class StorageWrapperTypeAttribute : Attribute
    {
        public StorageWrapperTypeAttribute(StorageType type)
        {
            Type = type;
        }

        private StorageType Type { get; }
    }
}