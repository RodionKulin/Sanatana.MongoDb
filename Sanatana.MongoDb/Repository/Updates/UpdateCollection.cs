using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.MongoDb.Repository
{
    public struct UpdateCollection<TEntity>
        where TEntity : class
    {
        public LambdaExpression PropertyExpression { get; set; }
        public object Value { get; set; }

        public static UpdateCollection<TEntity> Property<TProperty>(Expression<Func<TEntity, List<TProperty>>> propertyExpression, TProperty value)
        {
            return new UpdateCollection<TEntity>
            {
                PropertyExpression = propertyExpression,
                Value = value
            };
        }
    }
}
