using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Cauldron.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CardPool.WriteToFile(@"CardSet/sample_cardset_.json");
            //var a = CardPool.ReadFromFile(@"CardSet/sample_cardset.json");
            //return;

            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
