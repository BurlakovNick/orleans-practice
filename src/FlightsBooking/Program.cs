using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Orleans.Hosting;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace FlightsBooking
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            try
            {
                logger.Info($"STARTING!!!");

                var siloBuilder = new SiloHostBuilder()
                    .UseLocalhostClustering(serviceId: "local-orleans")
                    .AddAdoNetGrainStorage("SqlStorage", options =>
                    {
                        options.Invariant = "System.Data.SqlClient";
                        options.ConnectionString = "Data Source=.\\sql;Initial Catalog=orleans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                        options.UseJsonFormat = true;
                    })
                    .AddMemoryGrainStorageAsDefault()
                    .ConfigureLogging(logging => logging.AddNLog());

                using (var host = siloBuilder.Build())
                {
                    host.StartAsync().Wait();
                    BuildWebHost(args).Run();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog()
                .Build();
        }
    }
}
