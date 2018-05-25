using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sanatana.MongoDb.Identity
{
    public interface IUserQueries<TUser> 
        where TUser : MongoDbIdentityUser
    {
        Task<List<TUser>> SelectPage(int page, int pageSize);

        Task<TUser> FindByEmailAndName(string email, string name);
    }
}