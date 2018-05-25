using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{
    public class MongoDbUtility
    {
        public static int ToSkipNumber(int page, int pageSize)
        {
            if (pageSize < 1)
                throw new Exception("Number of items per page must be greater then 0.");

            if (page < 1)
                page = 1;

            return (page - 1) * pageSize;
        }

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
