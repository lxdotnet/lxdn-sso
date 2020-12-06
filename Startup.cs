using System;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

using Lxdn.Sso.Extensions;
using NLog;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Lxdn.Sso
{
    public class SpaRewriter
    {
        private readonly RequestDelegate next;
        private readonly IWebHostEnvironment host;

        public SpaRewriter(RequestDelegate next, IWebHostEnvironment host)
        {
            this.next = next;
            this.host = host;
        }

        public async Task InvokeAsync(HttpContext http)
        {
            if (http.Request.Path == "/spa")
            {
                var index = host.WebRootFileProvider.GetFileInfo("/index.html");
                http.Response.ContentType = "text/html";
                await http.Response.SendFileAsync(index);
                return;
            }

            await next(http);
        }
    }

    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();//.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            var jwk = configuration.GetSection("AppSettings:jwk").Get<JsonWebKey>(); // https://mkjwk.org/ https://tools.ietf.org/html/rfc7517#section-4
            var asymmetric = new SigningCredentials(jwk, jwk.Alg);

            IdentityModelEventSource.ShowPII = true; // include more information in the exceptions being returned (important for debugging)
            AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForSigningMap[jwk.Alg] = 512;
            AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap[jwk.Alg] = 512;

            services
                .AddCors()
                .AddHttpContextAccessor()
                .AddSingleton(LogManager.GetLogger("sso"))                
                .AddScoped(container => container.GetService<IHttpContextAccessor>().HttpContext)
                .AddAuthentication(IISDefaults.AuthenticationScheme);

            services.AddControllers();

            services
                .AddAuthentication(auth => {
                    auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    //auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/user/signin";
                    //options.LogoutPath = "/user/signout";
                })
                .AddOpenIdConnectServer(oauth => {
                    oauth.AllowInsecureHttp = true;
                    oauth.TokenEndpointPath = "/oauth/token";
                    oauth.AuthorizationEndpointPath = "/oauth/authorize";
                    oauth.ProviderType = typeof(AuthorizationProvider);
                    oauth.AccessTokenHandler = new JwtSecurityTokenHandler();
                    oauth.SigningCredentials.Add(asymmetric);
                    oauth.AccessTokenLifetime = TimeSpan.FromMinutes(15);
                    oauth.LogoutEndpointPath = "/user/signout";
                });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();
            app
            //.UseHttpsRedirection();
            .UseRouting()
            .UseAuthentication()
            .UseCors(Cors.AllowEverything)
            .UseDefaultFiles()
            .UseStaticFiles()
            .UseMiddleware<SpaRewriter>();
            //.UseEndpoints(endpoints => endpoints.MapControllers()); // https://stackoverflow.com/questions/57684093/using-usemvc-to-configure-mvc-is-not-supported-while-using-endpoint-routing
        }
    }
}
// https://stackoverflow.com/questions/46940710/getting-value-from-appsettings-json-in-net-core
// https://stackoverflow.com/questions/55361533/addidentity-vs-addidentitycore
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-2.2
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.0
// https://medium.com/@darutk/diagrams-and-movies-of-all-the-oauth-2-0-flows-194f3c3ade85

/*
  var symmetric = new SigningCredentials(
      new SymmetricSecurityKey(Encoding.ASCII.GetBytes("00001111000011110000111100001111")),
          SecurityAlgorithms.HmacSha256Signature,
          SecurityAlgorithms.Sha256Digest);
    // same symmetric key must be used when decryptng
 */
