using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.MongoDb.Repository
{
    public struct Update<T>
        where T : class
    {
        public Expression<Func<T, object>> PropertyExpression { get; set; }
        public object Value { get; set; }

        public static Update<T> Property(Expression<Func<T, object>> propertyExpression, object value)
        {
            return new Update<T>
            {
                PropertyExpression = propertyExpression,
                Value = value
            };
        }
    }

}
