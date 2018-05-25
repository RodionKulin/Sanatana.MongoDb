﻿using Sanatana.MongoDb;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sanatana.MongoDb.Identity
{
    public class MongoDbUserQueries<TUser> : IUserQueries<TUser>
        where TUser : MongoDbIdentityUser
    {
        //fields
        protected IdentityMongoDbContext<TUser> _context;


        //init
        public MongoDbUserQueries(MongoDbConnectionSettings settings)
        {
            _context = new IdentityMongoDbContext<TUser>(settings);
        }


        //methods
        public virtual async Task<List<TUser>> SelectPage(int page, int pageSize)
        {
            int skip = MongoDbUtility.ToSkipNumber(page, pageSize);

            FilterDefinition<TUser> filter = Builders<TUser>.Filter.Where(p => true);

            List<TUser> result = await _context.Users.Find(filter)
                .Limit(pageSize)
                .Skip(skip)
                .ToListAsync();

            return result;
        }

        public async Task<TUser> FindByEmailAndName(string email, string name)
        {
            FilterDefinition<TUser> filter = Builders<TUser>.Filter.Where(
                p => p.Email == email
                && p.UserName == name);

            TUser result = await _context.Users.Find(filter).FirstOrDefaultAsync();

            return result;
        }

    }
}