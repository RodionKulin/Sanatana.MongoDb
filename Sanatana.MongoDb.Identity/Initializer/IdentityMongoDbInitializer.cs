using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.MongoDb.Identity
{
    public class IdentityMongoDbInitializer<TUser>
        where TUser : MongoDbIdentityUser
    {
        //properties
        public IdentityMongoDbContext<TUser> Context { get; set; }


        //init
        public IdentityMongoDbInitializer(MongoDbConnectionSettings settings)
        {
            Context = new IdentityMongoDbContext<TUser>(settings);
        }


        //methods
        public void CreateAllIndexes()
        {
            CreateUsersIndex();
            CreateClaimsIndex();
            CreateUserLoginsIndex();
        }

        public void CreateUsersIndex()
        {
            IndexKeysDefinition<TUser> nameIndex = Builders<TUser>.IndexKeys
               .Ascending(p => p.UserName);

            CreateIndexOptions nameOptions = new CreateIndexOptions()
            {
                Name = "UserName",
                Unique = false
            };

            IndexKeysDefinition<TUser> emailIndex = Builders<TUser>.IndexKeys
               .Ascending(p => p.Email);

            CreateIndexOptions emailOptions = new CreateIndexOptions()
            {
                Name = "Email",
                Unique = false
            };

            IMongoCollection<TUser> collection = Context.Users;
            collection.Indexes.DropAllAsync().Wait();

            string indexName = collection.Indexes.CreateOneAsync(nameIndex, nameOptions).Result;
            string indexEmail = collection.Indexes.CreateOneAsync(emailIndex, emailOptions).Result;
        }

        public void CreateClaimsIndex()
        {
            IndexKeysDefinition<MongoDbUserClaim> userIdIndex = Builders<MongoDbUserClaim>.IndexKeys
               .Ascending(p => p.UserId);

            CreateIndexOptions userIdOptions = new CreateIndexOptions()
            {
                Name = "UserId",
                Unique = false
            };
            
            IMongoCollection<MongoDbUserClaim> collection = Context.Claims;
            collection.Indexes.DropAllAsync().Wait();

            string indexName = collection.Indexes.CreateOneAsync(userIdIndex, userIdOptions).Result;
        }

        public void CreateUserLoginsIndex()
        {
            IndexKeysDefinition<MongoDbUserLogin> userIdIndex = Builders<MongoDbUserLogin>.IndexKeys
               .Ascending(p => p.UserId);

            CreateIndexOptions userIdOptions = new CreateIndexOptions()
            {
                Name = "UserId",
                Unique = false
            };

            IndexKeysDefinition<MongoDbUserLogin> providerIndex = Builders<MongoDbUserLogin>.IndexKeys
               .Ascending(p => p.LoginProvider)
               .Ascending(p => p.ProviderKey);

            CreateIndexOptions providerOptions = new CreateIndexOptions()
            {
                Name = "LoginProvider + ProviderKey",
                Unique = true
            };

            IMongoCollection<MongoDbUserLogin> collection = Context.UserLogins;
            collection.Indexes.DropAllAsync().Wait();

            string loginIndexName = collection.Indexes.CreateOneAsync(userIdIndex, userIdOptions).Result;
            string providerIndexName = collection.Indexes.CreateOneAsync(providerIndex, providerOptions).Result;
        }
    }
}