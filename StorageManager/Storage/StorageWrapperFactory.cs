using System;
using System.Collections.Generic;
using StorageManager.Configuration;
using StorageManager.Enums;
using StorageManager.Exceptions;

namespace StorageManager.Storage
{
    public static class StorageWrapperFactory
    {
        private static readonly Dictionary<StorageType,Type> RegisteredWrappers = new Dictionary<StorageType, Type>();

        public static StorageWrapper<T> GetWrapper<T>(StorageEntityDefinition<T> definition, IStorageConfiguration configuration)
        {
            if(!RegisteredWrappers.ContainsKey(definition.StorageType))
                throw new StorageArgumentException($"Not found wrapper for storage type: {definition.StorageType}");

            return (StorageWrapper<T>) Activator.CreateInstance(RegisteredWrappers[definition.StorageType], definition, configuration);
        }

        public static void RegisterWrapper<T>(StorageType type) where T : StorageWrapper
        {
            if (RegisteredWrappers.ContainsKey(type))
                throw new StorageInvalidOperationException($"Already registered wrapper for storage type: {type}");
            RegisteredWrappers.Add(type, typeof(T) );
        }
    }
}