
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Unity.Microsoft.DependencyInjection;

namespace Lxdn.Sso
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Run();
        }

        public static IWebHost CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseUnityServiceProvider() // https://www.nuget.org/packages/Unity.Microsoft.DependencyInjection/ https://stackoverflow.com/questions/39173345/unity-with-asp-net-core-and-mvc6-core
            .UseStartup<Startup>()
            .Build();
    }
}

