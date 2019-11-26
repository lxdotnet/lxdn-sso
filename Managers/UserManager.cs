
using System.Threading.Tasks;

namespace Lxdn.Sso.Managers
{
    public class UserManager
    {
        public Task<bool> Validate(string username, string password)
        {
            if (password == "666")
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
    }
}
