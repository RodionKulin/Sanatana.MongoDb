using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Sanatana.MongoDb;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sanatana.MongoDbSpecs.Samples
{
    public class SamplesMongoDbContext
    {
        //fields
        private static bool _isMapped = false;
        private static object _mapLock = new object();
        protected MongoDbConnectionSettings _connectionSettings;

        //properties
        public IMongoDatabase Database { get; }
        public IMongoCollection<Post> Posts
        {
            get
            {
                return Database.GetCollection<Post>("Posts");
            }
        }

        //init
        public SamplesMongoDbContext(MongoDbConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
            Database = GetDatabase(connectionSettings);

            if (_isMapped == false)
            {
                lock (_mapLock)
                {
                    if (_isMapped == false)
                    {
                        _isMapped = true;
                        RegisterConventions();
                        MapSampleEntities();
                        ApplyGlobalSerializationSettings();
                    }
                }
            }
        }


        //methods
        public static void ApplyGlobalSerializationSettings()
        {
            var dateSerializer = new DateTimeSerializer(DateTimeKind.Utc);
            BsonSerializer.RegisterSerializer(typeof(DateTime), dateSerializer);
            var timeSpanSerializer = new TimeSpanNumberSerializer();
            BsonSerializer.RegisterSerializer(typeof(TimeSpan), timeSpanSerializer);

            BsonSerializer.UseNullIdChecker = true;
            BsonSerializer.UseZeroIdChecker = true;
        }

        private IMongoDatabase GetDatabase(MongoDbConnectionSettings connectionSettings)
        {
            var clientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress(connectionSettings.Host, connectionSettings.Port),
                WriteConcern = WriteConcern.Acknowledged,
                ReadPreference = ReadPreference.PrimaryPreferred,
                Credential = connectionSettings.Credential
            };

            clientSettings.ClusterConfigurator = cb => {
                cb.Subscribe<CommandStartedEvent>(e => {
                    System.Diagnostics.Trace.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
                });
            };

            MongoClient client = new MongoClient(clientSettings);
            return client.GetDatabase(connectionSettings.DatabaseName);
        }

        private void RegisterConventions()
        {
            var pack = new ConventionPack();
            pack.Add(new EnumRepresentationConvention(BsonType.Int32));
            pack.Add(new IgnoreIfNullConvention(true));
            pack.Add(new IgnoreIfDefaultConvention(false));

            Assembly entitiesAssembly = typeof(Post).GetTypeInfo().Assembly;
            ConventionRegistry.Register("ParsingTargets pack", pack,
                t => t.GetTypeInfo().Assembly == entitiesAssembly);
        }

        private void MapSampleEntities()
        {
            BsonClassMap.RegisterClassMap<Post>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.ID));
            });

        }

    }
}
