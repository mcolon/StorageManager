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

            var genericType = RegisteredWrappers[definition.StorageType];
            var storageWrapperType = genericType.MakeGenericType(typeof(T));

            return (StorageWrapper<T>) Activator.CreateInstance(storageWrapperType, definition, configuration);
        }

        public static void RegisterWrapper<T>(StorageType storageType) where T : StorageWrapper
        {
            var type = typeof(T);
            
            if(!typeof(StorageWrapper).IsAssignableFrom(type) || !type.IsGenericType)
                throw new StorageInvalidOperationException($"Cant use type: {type.Name} as StorageWrapper");

            if (RegisteredWrappers.ContainsKey(storageType))
                throw new StorageInvalidOperationException($"Already registered wrapper for storage type: {storageType}");

            RegisteredWrappers.Add(storageType, typeof(T) );
        }

        public static void RegisterWrapper(StorageType storageType, Type type) 
        {
            if(!typeof(StorageWrapper).IsAssignableFrom(type) || !type.IsGenericType)
                throw new StorageInvalidOperationException($"Cant use type: {type.Name} as StorageWrapper");

            if (RegisteredWrappers.ContainsKey(storageType))
                throw new StorageInvalidOperationException($"Already registered wrapper for storage type: {type}");
            RegisteredWrappers.Add(storageType, type );
        }
    }
}