using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Sitko.Core.App;

namespace Sitko.Core.Logging
{
    public class LoggingModule<T> : BaseApplicationModule<T> where T : LoggingOptions
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            Console.OutputEncoding = Encoding.UTF8;
            var logLevelSwitcher = new LogLevelSwitcher();
            Config.Facility = Config.Facility ?? environment.ApplicationName;
            var loggerConfiguration =
                new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("App", Config.Facility);


            Config.ConfigureLogger?.Invoke(loggerConfiguration, logLevelSwitcher);

            if (environment.IsDevelopment())
            {
                logLevelSwitcher.Switch.MinimumLevel = Config.DevLogLevel;
            }
            else
            {
                logLevelSwitcher.Switch.MinimumLevel = Config.ProdLogLevel;
                loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
            }

            if (Config.EnableConsoleLogging)
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Console(levelSwitch: logLevelSwitcher.Switch);
            }

            loggerConfiguration.MinimumLevel.ControlledBy(logLevelSwitcher.Switch);
            Log.Logger = loggerConfiguration.CreateLogger();
            services.AddSingleton(logLevelSwitcher);
            services.AddSingleton(_ => (ILoggerFactory)new SerilogLoggerFactory());
        }
    }
}