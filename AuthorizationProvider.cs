
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using AspNet.Security.OpenIdConnect.Server;
using Lxdn.Sso.Managers;

namespace Lxdn.Sso
{
    public class AuthorizationProvider : OpenIdConnectServerProvider
    {
        public AuthorizationProvider(IdentityManager identities, UserManager users, 
            GuardedExecution guard)
        { 
            OnValidateTokenRequest = context => guard.Action(() => {
                // validate client_id/secret
                context.Validate();
                return Task.CompletedTask;
            });

            OnHandleTokenRequest = async context => {
                if (! await users.Validate(context.Request.Username, context.Request.Password))
                {
                    context.Reject();
                }
                else
                {
                    var identity = await identities.Create(context.Request.Username);
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), new AuthenticationProperties(), context.Scheme.Name);
                    //ticket.SetScopes(OpenIdConnectConstants.Scopes.OpenId);
                    context.Validate(ticket);
                }
            };

            OnValidateAuthorizationRequest = context => {
                context.Validate();
                return Task.FromResult(0);
            };

            OnHandleAuthorizationRequest = async context => 
            {
                var authentication = await context.HttpContext.AuthenticateAsync();
                if (!authentication.Succeeded)
                {
                    await context.HttpContext.ChallengeAsync();
                }
                else
                {
                    await context.HttpContext.SignInAsync(OpenIdConnectServerDefaults.AuthenticationScheme,
                        authentication.Principal, authentication.Properties);
                }

                context.HandleResponse();
            };

            OnHandleLogoutRequest = async context => {
                await context.HttpContext.SignOutAsync();
                var redirect = context.HttpContext.Request.Query["redirect_uri"];
                context.HttpContext.Response.Redirect(redirect);
                context.HandleResponse();
            };
        }
    }
}

// https://github.com/aspnet-contrib/AspNet.Security.OpenIdConnect.Server
// https://kevinchalet.com/2016/07/13/creating-your-own-openid-connect-server-with-asos-creating-your-own-authorization-provider/
// https://stackoverflow.com/questions/41291729/angularjs-webapi-jwt-with-integrated-windows-authentication/41381517