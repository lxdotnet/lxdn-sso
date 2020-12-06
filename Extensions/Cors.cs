
using Microsoft.AspNetCore.Cors.Infrastructure;
using System;

namespace Lxdn.Sso.Extensions
{
    public static class Cors
    {
        public static Action<CorsPolicyBuilder> AllowEverything =>
            policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();//.AllowCredentials();
    }
}
