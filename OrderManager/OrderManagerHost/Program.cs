using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.NHibernateIntegration;
using MassTransit.NHibernateIntegration.Saga;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;
using OrderManager.Business.Observers;
using OrderManager.Business.Sagas;

namespace OrderManagerHost
{
    public class Program
    {
        public static IConfigurationRoot configuration;

        private static string ConnectionString;
        private static string RabbitHost;
        private static string RabbitUser;
        private static string RabbitPassword;
        private static string RabbitInputQueue;

        public static async Task Main()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var machine = new OrderStateMachine();
            var sessionFactory = serviceProvider.GetService<ISessionFactory>();
            var repository = NHibernateSagaRepository<OrderSagaState>.Create(sessionFactory);
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(RabbitHost, h =>
                {
                    h.Username(RabbitUser);
                    h.Password(RabbitPassword);
                });

                cfg.ReceiveEndpoint(RabbitInputQueue, e =>
                {
                    e.StateMachineSaga(machine, repository);
                });
            });
            var observer = new ReceiveObserver();
            var handle = busControl.ConnectReceiveObserver(observer);

            await busControl.StartAsync(CancellationToken.None);
            try
            {
                Console.WriteLine("Press enter to exit");
                await Task.Run(() => Console.ReadLine());
            }
            finally
            {
                await busControl.StopAsync(CancellationToken.None);
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
            serviceCollection.AddSingleton(configuration);

            ConnectionString = configuration.GetConnectionString("DefaultConnection");
            var appSettings = configuration.GetSection("AppSettings");
            RabbitHost = appSettings.GetValue("RabbitHost", string.Empty);
            RabbitUser = appSettings.GetValue("RabbitUser", string.Empty);
            RabbitPassword = appSettings.GetValue("RabbitPassword", string.Empty);
            RabbitInputQueue = appSettings.GetValue("RabbitInputQueue", string.Empty);

            var mappings = Assembly.Load("OrderManager.Business")
                .GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType &&
                    (t.BaseType.GetGenericTypeDefinition() == typeof(SagaClassMapping<>) ||
                    t.BaseType.GetGenericTypeDefinition() == typeof(ClassMapping<>)))
                .ToArray();
            serviceCollection.AddSingleton((cfg) =>
            {
                return new SqlServerSessionFactoryProvider(ConnectionString, mappings).GetSessionFactory();
            });
        }
    }
}
