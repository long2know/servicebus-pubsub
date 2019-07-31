using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Models;

namespace ServiceBusSpikePub
{
    public class Startup
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static class ServiceProviderFactory
        {
            public static IServiceProvider ServiceProvider { get; set; }
        }

        public static void Main(string[] args)
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string launch = Environment.GetEnvironmentVariable("LAUNCH_PROFILE");

            if (string.IsNullOrWhiteSpace(env))
            {
                env = "Development";
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (env == "Development")
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();

            // Create a service collection and configure our depdencies
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Build the our IServiceProvider
            ServiceProviderFactory.ServiceProvider = serviceCollection.BuildServiceProvider();

            // Grab our settings if we want to use them for anything ..
            var appSettings = (ServiceProviderFactory.ServiceProvider.GetService(typeof(IOptions<AppSettings>)) as IOptions<AppSettings>).Value;

            // Entry the applicaiton.. (run!)
            ServiceProviderFactory.ServiceProvider.GetService<Application>().Run().GetAwaiter().GetResult();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Make configuration settings available
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add caching
            services.AddMemoryCache();

            // Add logging            
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug();
            });

            // Add Application 
            services.AddTransient<Application>();
        }
    }
}
