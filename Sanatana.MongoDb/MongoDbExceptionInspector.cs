using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public class MongoDbExceptionInspector
    {
       

        public static bool IsDuplicateException(Exception exception, string fieldName = null)
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
    }
}
