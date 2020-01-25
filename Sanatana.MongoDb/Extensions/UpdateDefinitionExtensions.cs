using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public static class UpdateDefinitionExtensions
    {
        //fields
        private static Dictionary<Type, List<PropertyInfo>> _mappedProperties;


        //init
        static UpdateDefinitionExtensions()
        {
            _mappedProperties = new Dictionary<Type, List<PropertyInfo>>();
        }


        //methods
        public static UpdateDefinition<TDocument> SetOnInsertAllMappedMembers<TDocument>(
            this UpdateDefinitionBuilder<TDocument> updateBuilder, TDocument item
            , params Expression<Func<TDocument, object>>[] excludeMembers)
        {
            UpdateDefinition<TDocument> update = updateBuilder.Combine();
            return SetOnInsertAllMappedMembers<TDocument>(update, item, excludeMembers);
        }

        public static UpdateDefinition<TDocument> SetOnInsertAllMappedMembers<TDocument>(
            this UpdateDefinition<TDocument> update, TDocument item
            , params Expression<Func<TDocument, object>>[] excludeMembers)
        {
            excludeMembers = excludeMembers ?? new Expression<Func<TDocument, object>>[0];
            List<string> excludeMemberNames = excludeMembers.Select(p => GetMemberName(p)).ToList();

            Type itemType = typeof(TDocument);
            List<PropertyInfo> mappedMembers = GetMappedMembers(itemType);

            foreach (PropertyInfo prop in mappedMembers)
            {
                if (excludeMemberNames.Contains(prop.Name))
                    continue;

                object value = prop.GetValue(item);
                update = update.SetOnInsert(prop.Name, value);
            }

            return update;
        }

        public static UpdateDefinition<TDocument> SetAllMappedMembers<TDocument>(
            this UpdateDefinitionBuilder<TDocument> updateBuilder, TDocument item
            , params Expression<Func<TDocument, object>>[] excludeMembers)
        {
            UpdateDefinition<TDocument> update = updateBuilder.Combine();
            return SetAllMappedMembers<TDocument>(update, item, excludeMembers);
        }

        public static UpdateDefinition<TDocument> SetAllMappedMembers<TDocument>(
            this UpdateDefinition<TDocument> update, TDocument item
            , params Expression<Func<TDocument, object>>[] excludeMembers)
        {
            excludeMembers = excludeMembers ?? new Expression<Func<TDocument, object>>[0];
            List<string> excludeMemberNames = excludeMembers.Select(p => GetMemberName(p)).ToList();

            Type itemType = typeof(TDocument);
            List<PropertyInfo> mappedMembers = GetMappedMembers(itemType);

            foreach (PropertyInfo prop in mappedMembers)
            {
                if (excludeMemberNames.Contains(prop.Name))
                    continue;

                object value = prop.GetValue(item);
                update = update.Set(prop.Name, value);
            }

            return update;
        }

        private static string GetMemberName<TDocument>(Expression<Func<TDocument, object>> selectMemberLambda)
        {
            var member = selectMemberLambda.Body as MemberExpression;

            if (member == null)
            {
                var unaryExpression = selectMemberLambda.Body as UnaryExpression;
                var t = unaryExpression.Operand.GetType();
                member = unaryExpression.Operand as MemberExpression;
            }

            if (member == null)
            {
                throw new ArgumentException("The parameter selectMemberLambda must be a member accessing labda such as x => x.Id", "selectMemberLambda");
            }
            return member.Member.Name;
        }

        private static List<PropertyInfo> GetMappedMembers(Type itemType)
        {
            if (_mappedProperties.ContainsKey(itemType))
            {
                return _mappedProperties[itemType];
            }

            PropertyInfo[] itemProps = itemType.GetProperties();
            List<BsonClassMap> classMaps = new List<BsonClassMap>();
            Type baseType = itemType;

            do
            {
                BsonClassMap classMap = BsonClassMap.LookupClassMap(baseType);
                classMaps.Add(classMap);

                TypeInfo baseTypeInfo = baseType.GetTypeInfo();

                baseType = baseTypeInfo.BaseType != null && baseTypeInfo.BaseType != typeof(object)
                    ? baseTypeInfo.BaseType
                    : null;
            }
            while (baseType != null);

            List<PropertyInfo> mappedMembers = new List<PropertyInfo>();

            foreach (PropertyInfo prop in itemProps)
            {
                bool memberMapped = classMaps.Any(p => p.GetMemberMap(prop.Name) != null);
                if (memberMapped)
                {
                    mappedMembers.Add(prop);
                }
            }

            _mappedProperties[itemType] = mappedMembers;
            return mappedMembers;
        }

        public static BsonDocument Render<TDocument>(this UpdateDefinition<TDocument> update)
        {
            return (BsonDocument)update.Render(
                BsonSerializer.LookupSerializer<TDocument>(), BsonSerializer.SerializerRegistry);
        }
    }
}
