using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public class TimeSpanNumberSerializer : IBsonSerializer<TimeSpan>
    {
        //properties
        public Type ValueType
        {
            get { return typeof(TimeSpan); }
        }


        //methods
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeSpan value)
        {
            long timestamp = (long)value.TotalMilliseconds;
            context.Writer.WriteInt64(timestamp);
        }
        
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            TimeSpan timeSpanValue = (TimeSpan)value;
            Serialize(context, args, timeSpanValue);
        }

        public TimeSpan Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            long timestamp = 0;
            if (context.Reader.CurrentBsonType == MongoDB.Bson.BsonType.Int32)
            {
                timestamp = context.Reader.ReadInt32();
            }
            else if (context.Reader.CurrentBsonType == MongoDB.Bson.BsonType.Int64)
            {
                timestamp = context.Reader.ReadInt64();
            }
            else
            {
                string message = string.Format("Unknown timestamp bson type {0}", context.Reader.CurrentBsonType);
                throw new FormatException(message);
            }

            return TimeSpan.FromMilliseconds(timestamp);
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

    }
}
