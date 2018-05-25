using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public class ObjectIdTypeConverter : TypeConverter
    {

        //methods
        public override bool CanConvertFrom(
            ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string)
                ? true
                : base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(
            ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value is string
                ? new ObjectId(value as string)
                : base.ConvertFrom(context, culture, value);
        }

        public static void Register()
        {
            TypeDescriptor.AddAttributes(typeof(ObjectId)
                , new TypeConverterAttribute(typeof(ObjectIdTypeConverter)));
        }
    }
}
