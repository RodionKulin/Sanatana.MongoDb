using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.MongoDb.Repository
{
    public class EqualityExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            string equalsName = nameof(EqualityComparer<int>.Default.Equals);
            if (node.Method.Name == equalsName)
            {
                return Expression.Equal(
                    node.Arguments[0],
                    node.Arguments[1]);
            }

            return base.VisitMethodCall(node);
        }
    }
}
