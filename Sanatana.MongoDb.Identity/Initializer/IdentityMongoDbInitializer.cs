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
        public void DropAndCreateAllIndexes()
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
            collection.Indexes.DropAll();

            string indexName = collection.Indexes.CreateOne(nameIndex, nameOptions);
            string indexEmail = collection.Indexes.CreateOne(emailIndex, emailOptions);
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
            collection.Indexes.DropAll();

            string indexName = collection.Indexes.CreateOne(userIdIndex, userIdOptions);
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
            collection.Indexes.DropAll();

            string loginIndexName = collection.Indexes.CreateOne(userIdIndex, userIdOptions);
            string providerIndexName = collection.Indexes.CreateOne(providerIndex, providerOptions);
        }
    }
}