using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.MongoDbSpecs.Samples
{
    public class UniqueEntity
    {
        public ObjectId ID { get; set; }
        public string UniqueValue { get; set; }
    }
}
