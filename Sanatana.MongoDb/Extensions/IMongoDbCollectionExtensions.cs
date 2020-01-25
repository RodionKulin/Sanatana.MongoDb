using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using Sanatana.MongoDb.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public static class IMongoDbCollectionExtensions
    {
        //query analyze
        public static Task<BsonDocument> ExplainAggregation<TDocument, TResult>(
            this IMongoCollection<TDocument> collection
            , IAggregateFluent<TResult> aggregation, ExplainVerbosity verbosity)
        {
            IBsonSerializerRegistry serializerRegistry = collection.Settings.SerializerRegistry;
            IBsonSerializer<TDocument> serializer = serializerRegistry.GetSerializer<TDocument>();
            MessageEncoderSettings encoderSettings = collection.GetMessageEncoderSettings();
            
            var pipeline = new PipelineStagePipelineDefinition<TDocument, TResult>(aggregation.Stages);
            var renderedDefinition = pipeline.Render(serializer, serializerRegistry);

            var explainOperation = new AggregateOperation<TResult>(
                collection.CollectionNamespace,
                renderedDefinition.Documents,
                renderedDefinition.OutputSerializer,
                encoderSettings)
                .ToExplainOperation(verbosity);

            ICluster cluster = GetCluster(collection);
            ICoreSessionHandle session = NoCoreSession.NewHandle();
            using (IReadBinding binding = new ReadPreferenceBinding(cluster, collection.Settings.ReadPreference, session))
            {
                var cancelToken = new CancellationToken();
                return explainOperation.ExecuteAsync(binding, cancelToken);
            }
        }

        public static List<BsonDocument> GetAggregateStagesBson<TDocument, TResult>(
            this IMongoCollection<TDocument> collection, IAggregateFluent<TResult> aggregation)
        {
            IBsonSerializerRegistry serializerRegistry = collection.Settings.SerializerRegistry;
            IBsonSerializer<TDocument> serializer = serializerRegistry.GetSerializer<TDocument>();

            var pipeline = new PipelineStagePipelineDefinition<TDocument, TResult>(aggregation.Stages);
            var renderedDefinition = pipeline.Render(serializer, serializerRegistry);

            return renderedDefinition.Documents.ToList();
        }

        public static Func<object, object> GetIdFieldGetter<TDocument>(this IMongoCollection<TDocument> collection, Type itemType)
        {
            BsonClassMap classMap = BsonClassMap.LookupClassMap(itemType);
            Func<object, object> idGetter = classMap?.IdMemberMap?.Getter;
            if (idGetter == null)
            {
                throw new NullReferenceException($"Getter for MongoDb _id field not found for type {itemType.Name}");
            }
            return idGetter;
        }



        //collection state
        public static string GetStats<TDocument>(this IMongoCollection<TDocument> collection)
        {
            var readCommand = new BsonDocumentCommand<BsonDocument>(new BsonDocument()
            {
                { "collStats", collection.CollectionNamespace.CollectionName }, 
                //{ "scale", 1024 }, 
                { "verbose", true },
                { "indexDetails", true }
            });

            BsonDocument result = collection.Database.RunCommandAsync(readCommand).Result;
            return result.ToJsonIntended();
        }

        public static void ClearCache<TDocument>(this IMongoCollection<TDocument> collection)
        {
            var command = new BsonDocumentCommand<BsonDocument>(new BsonDocument()
            {
                { "planCacheClear", collection.CollectionNamespace.CollectionName }
            });

            BsonDocument result = collection.Database.RunCommandAsync(command).Result;
        }

        public static List<BsonDocument> GetIndexes<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return collection.Indexes.ListAsync().Result.ToListAsync().Result;
        }
        


        //private methods
        private static ICluster GetCluster<TDocument>(IMongoCollection<TDocument> collection)
        {
            Type clientType = typeof(MongoClient);
            FieldInfo clusterField = clientType.GetField("_cluster", BindingFlags.NonPublic | BindingFlags.Instance);
            ICluster cluster = (ICluster)clusterField.GetValue(collection.Database.Client);

            return cluster;
        }

        private static MessageEncoderSettings GetMessageEncoderSettings<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return new MessageEncoderSettings
            {
                { MessageEncoderSettingsName.GuidRepresentation, collection.Settings.GuidRepresentation },
                { MessageEncoderSettingsName.ReadEncoding, collection.Settings.ReadEncoding ?? Utf8Encodings.Strict },
                { MessageEncoderSettingsName.WriteEncoding, collection.Settings.WriteEncoding ?? Utf8Encodings.Strict }
            };
        }
    }
}
