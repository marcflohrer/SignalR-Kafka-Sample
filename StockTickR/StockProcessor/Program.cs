using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockProcessor.Repositories;
using StockProcessor.Repositories.Core;

namespace StockProcessor {
    public class Program {
        public static void Main (string[] args) {

            CreateWebHostBuilder (args)
                .Build ()
                .Run ();
        }

        public static IWebHostBuilder CreateWebHostBuilder (string[] args) =>
            WebHost.CreateDefaultBuilder (args)
            .UseKestrel ()
            .UseContentRoot (Directory.GetCurrentDirectory ())
            .ConfigureAppConfiguration ((hostingContext, config) => {
                var env = hostingContext.HostingEnvironment;
                config.AddJsonFile ("appsettings.json", optional : true, reloadOnChange : true);
                config.AddJsonFile ($"appsettings.{env.EnvironmentName}.json", optional : true, reloadOnChange : true);
                config.AddEnvironmentVariables ();
            })
            //.UseSerilog ()
            .UseStartup<Startup> ();
    }
}