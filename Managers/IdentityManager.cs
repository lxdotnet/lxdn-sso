
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Lxdn.Sso.Managers
{
    public class IdentityManager
    {
        public Task<ClaimsIdentity> GetIdentity(string user)
        {
            return Task.FromResult(new ClaimsIdentity()
                .AddClaim(OpenIdConnectConstants.Claims.Subject, user)
                .AddClaim("test1", "test2"));
        }
    }
}
