using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitko.Core.App;
using Sitko.Core.Db.InMemory;
using Sitko.Core.Db.Postgres;

namespace Sitko.Core.Xunit
{
    public abstract class DbBaseTestScope<TApplication, TScope, TDbContext> : BaseTestScope<TApplication>
        where TScope : class where TDbContext : DbContext where TApplication : Application<TApplication>
    {
        private TDbContext? _dbContext;

        protected override TApplication ConfigureApplication(TApplication application, string name)
        {
            base.ConfigureApplication(application, name);
            application.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddEnvironmentVariables()
                    .AddUserSecrets<TScope>();
            });
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<TScope>()
                .Build();

            var testInMemory =
                string.IsNullOrEmpty(config["CG_TESTS_POSTGRES"])
                || !bool.TryParse(config["CG_TESTS_POSTGRES"], out var outBool) || !outBool;
            if (testInMemory)
            {
                application.AddModule<InMemoryDatabaseModule<TDbContext>, InMemoryDatabaseModuleConfig<TDbContext>>(
                    (configuration, environment, moduleConfig) =>
                    {
                        moduleConfig.Database = name;
                        moduleConfig.Configure = (builder, provider, conf, env) =>
                        {
                            ConfigureInMemoryDatabaseModule(builder, conf, env);
                        };
                    });
            }
            else
            {
                application.AddModule<PostgresModule<TDbContext>, PostgresDatabaseModuleConfig<TDbContext>>((
                    configuration,
                    environment, moduleConfig) =>
                {
                    GetPostgresConfig(configuration, environment, moduleConfig, name);
                });
            }

            return application;
        }

        protected virtual void ConfigureInMemoryDatabaseModule(DbContextOptionsBuilder builder,
            IConfiguration configuration, IHostEnvironment environment)
        {
        }

        public override void OnCreated()
        {
            _dbContext = ServiceProvider.GetService<TDbContext>();
            if (_dbContext == null)
            {
                throw new Exception("Can't create db context");
            }

            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
            InitDbContext(_dbContext);
        }

        public TDbContext GetDbContext()
        {
            if (_dbContext == null)
            {
                throw new Exception("Db context is null");
            }

            return _dbContext;
        }

        protected virtual void InitDbContext(TDbContext dbContext)
        {
        }

        protected virtual void GetPostgresConfig(IConfiguration configuration,
            IHostEnvironment environment, PostgresDatabaseModuleConfig<TDbContext> moduleConfig, string dbName)
        {
            throw new NotImplementedException("You need to implement postgres configuration in your scope");
        }

        public override async ValueTask DisposeAsync()
        {
            if (_dbContext != null)
            {
                await _dbContext.Database.EnsureDeletedAsync();
            }

            await base.DisposeAsync();
        }
    }

    public abstract class DbBaseTestScope<TScope, TDbContext> : DbBaseTestScope<TestApplication, TScope, TDbContext>
        where TScope : class where TDbContext : DbContext
    {
    }
}
