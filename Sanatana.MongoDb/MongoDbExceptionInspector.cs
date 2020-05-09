using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public class MongoDbExceptionInspector
    {
        /// <summary>
        /// Check if exception was thown because of unique index constraint
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static bool IsDuplicateException(Exception exception)
        {
            if (exception == null
               || exception.InnerException == null)
            {
                return false;
            }

            bool isDup = exception.InnerException.Message.Contains("E11000 duplicate");
            return isDup;
        }

        /// <summary>
        /// Check if exception was thown because of unique index constraint on specific property
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool IsDuplicateException(Exception exception, string fieldName)
        {
            if (exception == null
               || exception.InnerException == null)
            {
                return false;
            }

            bool isDup = exception.InnerException.Message.Contains("E11000 duplicate");

            bool fieldMatched = !string.IsNullOrEmpty(fieldName)
                && exception.InnerException.Message.Contains(".$" + fieldName);

            return isDup && fieldMatched;
        }

        /// <summary>
        /// Check if exception was thown because of unique index constraint on specific property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exception"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsDuplicateException<T>(Exception exception, Expression<Func<T, object>> property)
        {
            string fieldName = FieldDefinitions.GetFieldMappedName(property);
            return IsDuplicateException(exception, fieldName);
        }


    }
}
