using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.MongoDb
{
    public static class MongoDbPageNumbers
    {
        /// <summary>
        /// Get number of items to skip
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize">number of pages per page</param>
        /// <returns></returns>
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
