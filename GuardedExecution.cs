
using System;
using System.Linq;
using System.Threading.Tasks;

using NLog;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

using Lxdn.Core.Basics;
using Lxdn.Core.Extensions;

namespace Lxdn.Sso
{
    public class GuardedExecution
    {
        private readonly Logger logger;
        private readonly HttpContext http;

        public GuardedExecution(Logger logger, HttpContext http)
        {
            this.logger = logger;
            this.http = http;
        }

        public async Task Action(Func<Task> asyncAction)
        {
            try 
            { 
                await asyncAction();
            }
            catch (Exception ex)
            {
                var url = http.Request.Scheme + "://" + http.Request.Host.Value + http.Request.Path;
                if (http.Request.QueryString.HasValue)
                    url = url + "?" + http.Request.QueryString;

                var form = http.Request.Form
                    .Without(x => string.Equals(x.Key, "password"))
                    .Aggregate(new CaseInsensitiveExpando(), (expando, field) 
                    => expando.Set(field.Key, field.Value.ToString()));
                
                logger.Error(ex, JsonConvert.SerializeObject(new { url, form }));
                throw;
            }
        }
    }
}
