using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public static class BsonSize
    {
        private const long OneKb = 1024;
        private const long OneMb = OneKb * 1024;
        private const long OneGb = OneMb * 1024;
        private const long OneTb = OneGb * 1024;

        public static long ToSize(this BsonDocument bsonDocument)
        {
            byte[] bsonBytes = bsonDocument.ToBson();
            return bsonBytes.Length;
        }

        public static string ToPrettySize(this byte[] bsonBytes, int decimalPlaces = 0)
        {
            return bsonBytes.Length.ToPrettySize(decimalPlaces);
        }

        public static string ToPrettySize(this BsonDocument bsonDocument, int decimalPlaces = 0)
        {
            byte[] bsonBytes = bsonDocument.ToBson();
            return bsonBytes.ToPrettySize(decimalPlaces);
        }

        public static string ToPrettySize(this int value, int decimalPlaces = 0)
        {
            return ((long)value).ToPrettySize(decimalPlaces);
        }

        public static string ToPrettySize(this long value, int decimalPlaces = 0)
        {
            var asTb = Math.Round((double)value / OneTb, decimalPlaces);
            var asGb = Math.Round((double)value / OneGb, decimalPlaces);
            var asMb = Math.Round((double)value / OneMb, decimalPlaces);
            var asKb = Math.Round((double)value / OneKb, decimalPlaces);
            string chosenValue = asTb > 1 ? string.Format("{0}Tb", asTb)
                : asGb > 1 ? string.Format("{0}Gb", asGb)
                : asMb > 1 ? string.Format("{0}Mb", asMb)
                : asKb > 1 ? string.Format("{0}Kb", asKb)
                : string.Format("{0}B", Math.Round((double)value, decimalPlaces));
            return chosenValue;
        }

        public static long GetBatchSize(this IAsyncCursor<BsonDocument> cursor)
        {
            cursor.MoveNextAsync().Wait();

            long totalSize = 0;

            foreach (BsonDocument item in cursor.Current)
            {
                totalSize += item.ToSize();
            }

            return totalSize;
        }

        public static long GetFirstDocumentSize(this IAsyncCursor<BsonDocument> cursor)
        {
            cursor.MoveNextAsync().Wait();

            BsonDocument document = cursor.Current.FirstOrDefault();
            return document.ToSize();
        }
    }
}
