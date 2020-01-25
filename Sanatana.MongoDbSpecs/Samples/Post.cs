using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.MongoDbSpecs.Samples
{
    public class Post
    {
        public ObjectId ID { get; set; }
        public string Text { get; set; }
        public int Counter { get; set; }
        public List<int> History { get; set; }
        public DateTime CreatedTimeUtc { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
