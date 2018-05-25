using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb.Identity
{
    public class MongoDbUserClaim
    {
        public ObjectId Id { get; set; }

        public ObjectId UserId { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public string Issuer { get; set; }

        public string OriginalIssuer { get; set; }
    }
}