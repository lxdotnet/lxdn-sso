using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Lxdn.Sso.Managers;
using Lxdn.Sso.ViewModels.Authentication;

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
            if (! await users.Exists(user.Username, user.Password))
            {
                ModelState.AddModelError("Error", "Authentication failed");
                return View("SignIn");
            }

            var identity = await identities.GetIdentity(user.Username);
            var properties = new AuthenticationProperties { IsPersistent = user.RememberMe };
            return SignIn(new ClaimsPrincipal(identity), properties, JwtBearerDefaults.AuthenticationScheme);
        }

        [HttpGet("user/signout")]
        public ActionResult SignOut()
        {
            SignOut(JwtBearerDefaults.AuthenticationScheme);
            return Redirect(Request.Query["redirect_uri"]);
        }
    }
}

// test url to present the login form:
// http://localhost:58225/authentication/login?ReturnUrl=%2Foauth%2Fauthorize%3Fclient_id%3Dabc%26redirect_uri%3Dhttp%253A%252F%252Flocalhost%252Fdef%26response_type%3Dtoken
// url encode: https://www.urlencoder.org/
// http://localhost:58225/user/signin?ReturnUrl=%2Foauth%2Fauthorize%3Fclient_id%3Dabc%26redirect_uri%3Dhttp%253A%252F%252Flocalhost%252Fdef%26response_type%3Dtoken