using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using StockProcessor.Hubs;
using StockProcessor.Repositories;
using StockProcessor.Repositories.Interfaces;
using StockProcessor.Subscriptions;
using StocksProcessor.Controllers;

namespace StockProcessor {
    public class Startup {
        public Startup (IHostingEnvironment env) {
            Configuration = new ConfigurationBuilder ()
                .SetBasePath (env.ContentRootPath)
                .AddJsonFile ("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile ($"appsettings.{env.EnvironmentName}.json")
                .AddEnvironmentVariables ()
                .Build ();

            HostingEnvironment = env;
            ConnectionString = string.Format (
                Configuration.GetConnectionString ("DefaultConnection"),
                Configuration["SA_PASSWORD"]);
        }

        public Startup (IConfigurationRoot configuration, IHostingEnvironment hostingEnvironment) {
            this.Configuration = configuration;
            this.HostingEnvironment = hostingEnvironment;

        }
        public IConfigurationRoot Configuration {
            get;
        }
        public IHostingEnvironment HostingEnvironment {
            get;
        }
        public string ConnectionString {
            get;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.Configure<CookiePolicyOptions> (options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            Log.Logger = new LoggerConfiguration ()
                .ReadFrom.Configuration (Configuration.GetSection ("Logging"))
                .MinimumLevel.Override ("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console (outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {EventId} {Message:lj} {Properties}{NewLine}{Exception}{NewLine}")
                .CreateLogger ();

            services.AddSingleton (Log.Logger);
            services.AddLogging (loggingBuilder => loggingBuilder.AddSerilog (dispose: true));
            services.AddSingleton (Configuration);

            services.AddScoped<StocksController, StocksController> ();
            services.AddSingleton<StockProcessorSubscription> ();

            // Add IHubContext's to the dependency container using AddSignalR()
            services.AddSignalR ();

            services.AddMvc ()
                .SetCompatibilityVersion (CompatibilityVersion.Version_2_1)
                .AddRazorPagesOptions (o => {
                    o.Conventions.ConfigureFilter (new IgnoreAntiforgeryTokenAttribute ());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime appLifetime) {
            // Ensure any buffered events are sent at shutdown
            appLifetime.ApplicationStopped.Register (Log.CloseAndFlush);

            app.UseStatusCodePagesWithReExecute ("/Home/Error", "?statusCode={0}");

            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseExceptionHandler ("/Error");
            }

            app.UseStaticFiles ();

            app.UseSignalR (routes => {
                routes.MapHub<StockHub> ("/stockshub");
            });

            app.UseMvc ();
        }
    }
}