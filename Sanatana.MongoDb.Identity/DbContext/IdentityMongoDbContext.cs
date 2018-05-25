using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb.Identity
{
    public class IdentityMongoDbContext<TUser>
        where TUser : MongoDbIdentityUser
    {
        //fields
        private static bool _isMapped = false;
        private static object _mapLock = new object();
        private IMongoDatabase _database;


        //properties
        public virtual IMongoCollection<TUser> Users
        {
            get
            {
                return _database.GetCollection<TUser>("Users");
            }
        }

        public virtual IMongoCollection<MongoDbUserClaim> Claims
        {
            get
            {
                return _database.GetCollection<MongoDbUserClaim>("Claims");
            }
        }

        public virtual IMongoCollection<MongoDbUserLogin> UserLogins
        {
            get
            {
                return _database.GetCollection<MongoDbUserLogin>("UserLogins");
            }
        }

        
        
        //init
        public IdentityMongoDbContext(MongoDbConnectionSettings settings)
        {
            _database = GetMongoDatabase(settings);

            lock (_mapLock)
            {
                if (!_isMapped)
                {
                    _isMapped = true;
                    RegisterConventions();
                    MapEntities();
                }
            }
        }


        //methods
        public static void ApplyGlobalSerializationSettings()
        {
            var dateSerializer = new DateTimeSerializer(DateTimeKind.Utc);
            BsonSerializer.RegisterSerializer(typeof(DateTime), dateSerializer);
            
            BsonSerializer.UseNullIdChecker = true;
            BsonSerializer.UseZeroIdChecker = true;
        }

        protected virtual IMongoDatabase GetMongoDatabase(MongoDbConnectionSettings settings)
        {
            var clientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress(settings.Host, settings.Port),
                WriteConcern = WriteConcern.Acknowledged,
                ReadPreference = ReadPreference.PrimaryPreferred,
                Credential = settings.Credential,
                
            };
            
            MongoClient client = new MongoClient(clientSettings);
            return client.GetDatabase(settings.DatabaseName);
        }

        protected virtual void RegisterConventions()
        {
            var pack = new ConventionPack();
            pack.Add(new EnumRepresentationConvention(BsonType.Int32));
            pack.Add(new IgnoreIfNullConvention(true));
            pack.Add(new IgnoreIfDefaultConvention(false));

            Assembly thisAssembly = typeof(IdentityMongoDbContext<>).GetTypeInfo().Assembly;
            ConventionRegistry.Register("Identity custom pack",
                pack,
                t => t.GetTypeInfo().Assembly == thisAssembly);
        }

        protected virtual void MapEntities()
        {
            BsonClassMap.RegisterClassMap<TUser>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.Id));
                cm.SetIgnoreExtraElements(true);
            });

            if(typeof(TUser) != typeof(MongoDbIdentityUser))
            {
                BsonClassMap.RegisterClassMap<MongoDbIdentityUser>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(m => m.Id));
                    cm.SetIgnoreExtraElements(true);
                });
            }

            BsonClassMap.RegisterClassMap<MongoDbUserClaim>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.Id));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<MongoDbUserLogin>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

    }
}
