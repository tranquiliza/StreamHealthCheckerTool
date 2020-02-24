using StreamHealthChecker.Core;
using StreamHealthChecker.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamHealthChecker.Repository.Sql
{
    // TODO Implement actual user repository
    public class UserRepository : IUserRepository
    {
        private readonly IEnumerable<User> _users;

        public UserRepository(string tranqUrl)
        {
            var tranquiliza = new User("Tranquiliza", 1000, tranqUrl);
            tranquiliza.AddNoConnectionMessage("Tranq come back BibleThump ");
            tranquiliza.AddNoConnectionMessage("Tranq you need to come back to the grid! D:");

            tranquiliza.AddPoorConnectionMessage("Tranq is still having issues..");
            tranquiliza.AddPoorConnectionMessage("God Tranq, get your ... together.");

            _users = new List<User>
            {
                tranquiliza
            };
        }

        public Task<IEnumerable<User>> GetUsers()
        {
            return Task.FromResult(_users);
        }
    }
}
