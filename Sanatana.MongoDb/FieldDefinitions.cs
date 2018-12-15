﻿using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public static class FieldDefinitions
    {
        public static FieldDefinition<TDocument> CreateField<TDocument, TItem>(
            Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            return new ExpressionFieldDefinition<TDocument>(field);
        }

        public static FieldDefinition<TDocument> CreateField<TDocument>(
            Expression<Func<TDocument, object>> field)
        {
            return new ExpressionFieldDefinition<TDocument>(field);
        }

        public static string GetFieldMappedName<TDocument>(Expression<Func<TDocument, object>> field)
        {
            var defin = FieldDefinitions.CreateField<TDocument>(field);
            return GetFieldMappedName<TDocument>(defin);
        }

        public static string GetFieldMappedName<TDocument>(
           FieldDefinition<TDocument> fieldDefinition)
        {
            RenderedFieldDefinition renderDefinition = fieldDefinition
                .Render(BsonSerializer.LookupSerializer<TDocument>()
                , BsonSerializer.SerializerRegistry);
            return renderDefinition.FieldName;
        }

    }
}