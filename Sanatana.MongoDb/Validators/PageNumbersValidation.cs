using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.MongoDb.Validators
{
    public static class PageNumbersValidation
    {
        public static int ToSkipNumber(int pageIndex, int pageSize)
        {
            if (pageSize < 1)
            {
                throw new ArgumentException($"{nameof(pageSize)} should be greater than 0.");
            }

            if (pageIndex < 0)
            {
                throw new ArgumentException($"{nameof(pageIndex)} should be equal or greater than 0.");
            }

            return pageIndex * pageSize;
        }
    }
}
