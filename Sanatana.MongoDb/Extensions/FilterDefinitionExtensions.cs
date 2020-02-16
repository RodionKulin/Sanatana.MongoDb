using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.MongoDb.Extensions
{
    public static class FilterDefinitionExtensions
    {
        //methods
        public static string ToJson<TDocument>(this FilterDefinition<TDocument> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<TDocument>();
            BsonDocument bson = filter.Render(documentSerializer, serializerRegistry);
            return bson.ToJson();
        }
    }
}
