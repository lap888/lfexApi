using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace yoyoApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                    WebHost.CreateDefaultBuilder(args)
                    // #if DEBUG
                    //     .UseUrls("http://192.168.1.8:5012")
                    // #endif
                    .ConfigureLogging(configureLogging =>
                    {
                        configureLogging.AddFile();
                    })
                    .UseStartup<Startup>();
    }
}