using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CRaft
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    int defaultPort = 5001;
                    if (args.Any(s => s.StartsWith("--port=")))
                    {
                        var portSwitch = args.First(s => s.StartsWith("--port="));
                        defaultPort = int.Parse(portSwitch.Split('=').Last());
                    }

                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.ListenLocalhost(defaultPort, kOptions =>
                        {
                            kOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                            kOptions.UseHttps();
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
