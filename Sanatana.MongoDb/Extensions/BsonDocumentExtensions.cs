using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public static class BsonDocumentExtensions
    {
        public static string ToJsonIntended(this BsonDocument document)
        {
            return document.ToJson(new JsonWriterSettings()
            {
                Indent = true
            });
        }

    }
}
