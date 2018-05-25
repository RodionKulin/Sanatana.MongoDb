using System.Security.Claims;
using System.Threading.Tasks;
using System;
using MongoDB.Bson;

namespace Sanatana.MongoDb.Identity
{
    public class MongoDbIdentityUser
    {

        public ObjectId Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string PasswordHash { get; set; }

        public string Email { get; set; }

        public string NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool LockoutEnabled { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }
                
        public string SecurityStamp { get; set; }

        public int AccessFailedCount { get; set; }
     
        public string PhoneNumber { get; set; }
     
        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

    }
}