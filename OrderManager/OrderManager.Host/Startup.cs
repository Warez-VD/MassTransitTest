using MassTransit;
using MassTransit.NHibernateIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentMigrator.Runner;
using System.Reflection;
using Microsoft.OpenApi.Models;
using OrderManager.Business.Sagas;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using OrderManager.Business.Observers;

namespace OrderManagerHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        string ConnectionString => this.Configuration.GetConnectionString("DefaultConnection");

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(c => 
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "OrderManager",
                    Version = "1.0"
                });
            });

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSqlServer2016()
                    .WithGlobalConnectionString(ConnectionString)
                    .ScanIn(Assembly.Load("OrderManager.Data")).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            var mappings = Assembly.Load("OrderManager.Business")
                .GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType &&
                    (t.BaseType.GetGenericTypeDefinition() == typeof(SagaClassMapping<>) ||
                    t.BaseType.GetGenericTypeDefinition() == typeof(ClassMapping<>)))
                .ToArray();
            services.AddSingleton((cfg) => 
            {
                return new SqlServerSessionFactoryProvider(ConnectionString, mappings).GetSessionFactory();
            });
            services.AddMassTransit(x => 
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderSagaState>()
                    .NHibernateRepository();

                x.UsingRabbitMq((context, cfg) => 
                {
                    cfg.ConnectReceiveObserver(new ReceiveObserver());
                    cfg.ReceiveEndpoint("event-listener", e => 
                    {
                        e.ConfigureSaga<OrderSagaState>(context);
                    });
                });
            });
            
            services.AddMassTransitHostedService();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var swaggerEndpoint = "./v1/api-docs";
            app.UseSwagger(c => c.RouteTemplate = "api/docs/{documentName}/api-docs");
            app.UseSwaggerUI(c => 
            {
                c.RoutePrefix = "api/docs";
                c.SwaggerEndpoint(swaggerEndpoint, "OrderManager");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var scope = app.ApplicationServices.CreateScope();
            var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
            runner.ListMigrations();
            runner.MigrateUp();
        }
    }
}
