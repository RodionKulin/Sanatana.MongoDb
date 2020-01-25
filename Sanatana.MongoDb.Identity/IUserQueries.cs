using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sanatana.MongoDb.Identity
{
    public interface IUserQueries<TUser> 
        where TUser : MongoDbIdentityUser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page idnex</param>
        /// <param name="pageSize">Number of items on page</param>
        /// <returns></returns>
        Task<List<TUser>> SelectPage(int pageIndex, int pageSize);

        Task<TUser> FindByEmailAndName(string email, string name);
    }
}