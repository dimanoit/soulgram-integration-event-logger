# Integration events logger for Soulgram 
NuGet Package to handle db operations with Integration Events (Cutted event sourcing functionallity)

It's necessary to register dependencies in client app
```c#
 private void AddEventLogsDependencies(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("EventLogDb");
        services.AddDbContext<IntegrationEventLogContext>(
            o => o.UseSqlServer(connectionString));

        services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService>();
    }
```
