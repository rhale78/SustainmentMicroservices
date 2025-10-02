using ApplicationRegistry.BackgroundServices;
using ApplicationRegistry.Interfaces;
using ApplicationRegistry.Repository;
using ApplicationRegistry.Services;

namespace ApplicationRegistry.Microservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHealthChecks();

            builder.Services.AddHostedService<HealthyURLBackgroundService>();
            builder.Services.AddHostedService<NotHealthyURLBackgroundService>();
            builder.Services.AddSingleton<ApplicationRegistryService>();
            builder.Services.AddSingleton<IApplicationRegistryRepository, ApplicationRegistryRepository>();
            builder.Services.AddSingleton<ApplicationDiscoveryService>();
            builder.Services.AddSingleton<IApplicationDiscoveryRepository, ApplicationDiscoveryRepository>();
            builder.Services.AddSingleton<IApplicationDiscoveryService, ApplicationDiscoveryService>();
            //builder.Services.AddSingleton<IApplicationRegistryService,ApplicationRegistryService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseHealthChecks("/microserviceHealth");
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
