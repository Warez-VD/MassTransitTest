using System;
using System.IO;
using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderManager.Migrations.Runner
{
    public class Program
    {
        public static IConfigurationRoot configuration;

        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
            if (runner.HasMigrationsToApplyUp())
            {
                runner.ListMigrations();
                runner.MigrateUp();
            }
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            serviceCollection
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSqlServer2016()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());
        }
    }
}
