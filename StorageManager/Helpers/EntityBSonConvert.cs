using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using StorageManager.Exceptions;

namespace StorageManager.Helpers
{
    public static class BSonConvert
    {
        public static byte[] SerializeObject<T>(T element)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    DateParseHandling = DateParseHandling.DateTime,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                using (BsonDataWriter writer = new BsonDataWriter(ms))
                {
                    serializer.Serialize(writer, element);
                    ms.Seek(0, SeekOrigin.Begin);

                    byte[] originalContent = ms.ToArray();

                    return originalContent;
                }
            }
        }

        public static async Task<byte[]> SerializeObjectAsync<T>(T element)
        {
            return await Task.FromResult(SerializeObject<T>(element)).ConfigureAwait(false);
        }

        public static async Task<TEntity> DeserializeObjectAsync<TEntity>(byte[] document)
        {
            return await Task.FromResult<TEntity>(DeserializeObject<TEntity>(document)).ConfigureAwait(false);

        }
        public static TEntity DeserializeObject<TEntity>(byte[] document)
        {
            if (document == null)
            {
                throw new StorageArgumentNullException("document cannot be null");
            }

            using (MemoryStream ms = new MemoryStream(document))
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    NullValueHandling = NullValueHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    DateParseHandling = DateParseHandling.DateTime,
                    TypeNameHandling = TypeNameHandling.Auto,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                };

                using (BsonDataReader reader = new BsonDataReader(ms, false, DateTimeKind.Utc))
                {

                    TEntity entity = serializer.Deserialize<TEntity>(reader);
                    return entity;
                }
            }
        }

        public static async Task<IEnumerable<TEntity>> DeserializeCollectionAsync<TEntity>(IEnumerable<byte[]> documents)
        {
            if (documents == null)
            {
                throw new StorageArgumentNullException("documents", "Collection cannot be null");
            }

            List<TEntity> tableDocuments = new List<TEntity>();
            foreach (byte[] document in documents)
            {
                TEntity entity = await DeserializeObjectAsync<TEntity>(document).ConfigureAwait(false);
                tableDocuments.Add(entity);
            }
            return tableDocuments;
        }

        public static string SerializeToBase64String<T>(T toConvert)
        {
            var binaryData = SerializeObject(toConvert);
            return Convert.ToBase64String(binaryData);
        }
        public static T DeserializeFromBase64String<T>(string toConvert)
        {
            var binaryData = Convert.FromBase64String(toConvert);
            return DeserializeObject<T>(binaryData);
        }
    }
}