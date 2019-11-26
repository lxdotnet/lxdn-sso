using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Lxdn.Sso.Managers;
using Lxdn.Sso.ViewModels.Authentication;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Lxdn.Sso.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IdentityManager identities;
        private readonly UserManager users;

        public AuthenticationController(IdentityManager identities, UserManager users)
        {
            this.identities = identities;
            this.users = users;
        }

        [HttpGet("user/signin")]
        public ActionResult ShowSignIn()
        {
            return View("SignIn", new AuthenticationModel());
        }

        [HttpPost("user/signin")]
        public async Task<ActionResult> SignIn(AuthenticationModel user)
        {
            if (! await users.Validate(user.Username, user.Password))
            {
                ModelState.AddModelError("Error", "Authentication failed");
                return View("SignIn");
            }

            var identity = await identities.Create(user.Username);
            var properties = new AuthenticationProperties { IsPersistent = user.RememberMe };
            return SignIn(new ClaimsPrincipal(identity), properties, JwtBearerDefaults.AuthenticationScheme);
        }
    }
}

// test url to present the login form:
// http://localhost:58225/authentication/login?ReturnUrl=%2Foauth%2Fauthorize%3Fclient_id%3Dabc%26redirect_uri%3Dhttp%253A%252F%252Flocalhost%252Fdef%26response_type%3Dtoken
// url encode: https://www.urlencoder.org/
// http://localhost:58225/user/signin?ReturnUrl=%2Foauth%2Fauthorize%3Fclient_id%3Dabc%26redirect_uri%3Dhttp%253A%252F%252Flocalhost%252Fdef%26response_type%3Dtoken

// redirect:
// http://localhost:44396/#token_type=Bearer&access_token=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhYWEiLCJ0ZXN0MSI6InRlc3QyIiwidG9rZW5fdXNhZ2UiOiJhY2Nlc3NfdG9rZW4iLCJqdGkiOiIzYzhjMGUwNC1iOTAzLTQyNTYtODhlYy1hODc0MWJkODgxZWUiLCJhenAiOiJhYmMiLCJuYmYiOjE1NzIxNzkwNzksImV4cCI6MTU3MjE4MjY3OSwiaWF0IjoxNTcyMTc5MDc5LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjU4MjI1LyJ9.qS4CjJ6P5_QfDCFg28eDqpcLsuZ4-4GDurdenMpCUVvVDtd1TOOEYPb1kWnmtM9ThTVngHPKlrS9KAhFqbfdFNoPDiifmBH7KIoPmKBuvZiTpCZqxTex2iSjUc8dhc5ockgwVEmkwpckL2wKdT2ueibxb-cp5OCgCmI1GeXFtag&expires_in=3600