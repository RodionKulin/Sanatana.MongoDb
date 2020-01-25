using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Sanatana.MongoDb.Repository.Memory
{
    public static class ExpressionReflection
    {

        public static PropertyInfo GetPropertyInfo(Expression expression)
        {
            if(expression.NodeType == ExpressionType.Convert)
            {
                var convert = expression as UnaryExpression;
                expression = convert.Operand;
            }

            var memberAccess = (MemberExpression)expression;
            PropertyInfo property = (PropertyInfo)memberAccess.Member;
            return property;
        }

        public static Type GetElementType(IEnumerable source)
        {
            Type enumerableType = source.GetType();
            if (enumerableType.IsArray)
            {
                return enumerableType.GetElementType();
            }
            if (enumerableType.IsGenericType)
            {
                return enumerableType.GetGenericArguments().First();
            }
            return null;
        }

        public static void AddToEnumerable(IEnumerable source, object newItemToAdd)
        {
            Type elementType = GetElementType(source);
            var genericListType = typeof(List<>).MakeGenericType(elementType);

            MethodInfo addMethod = genericListType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
            addMethod.Invoke(source, new[] { newItemToAdd });
        }

        public static void RemoveFromEnumerable(IEnumerable source, object itemToRemove)
        {
            Type elementType = GetElementType(source);
            var genericListType = typeof(List<>).MakeGenericType(elementType);

            MethodInfo addMethod = genericListType.GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance);
            addMethod.Invoke(source, new[] { itemToRemove });
        }
    }
}
