using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server
{
    public class Program
    {
        public static readonly Random Random = new Random();
        public static T RandomPick<T>(IReadOnlyList<T> source) => source.Any() ? source[Program.Random.Next(source.Count)] : default;
        public static IEnumerable<T> RandomPick<T>(IReadOnlyList<T> source, int picksNum)
        {
            if (source.Count <= picksNum) return source;

            var indexList = Enumerable.Range(0, source.Count).ToList();
            var pickedList = new List<T>();
            for (var i = 0; i < picksNum; i++)
            {
                var pickedIndex = RandomPick(indexList);
                pickedList.Add(source[pickedIndex]);
                indexList.Remove(pickedIndex);
            }

            return pickedList;
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddFilter("Microsoft", LogLevel.None);
                    logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
                    logging.AddFilter("Grpc", LogLevel.None);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
