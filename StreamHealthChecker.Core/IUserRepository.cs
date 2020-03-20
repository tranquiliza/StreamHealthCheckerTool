using StreamHealthChecker.Core.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamHealthChecker.Core
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUsers();
    }
}
