using Microsoft.Extensions.Logging;

namespace EfCoreMigrationApp.Services;

/// <summary>
/// Seed orchestrator to manage and execute seed services
/// </summary>
public class SeedOrchestrator(
    IEnumerable<ISeedService> seedServices,
    ILogger<SeedOrchestrator> logger)
{
    private readonly IEnumerable<ISeedService> _seedServices = seedServices;
    private readonly ILogger<SeedOrchestrator> _logger = logger;

    /// <summary>
    /// Executes all seed services
    /// </summary>
    public async Task ExecuteAllAsync()
    {
        var orderedServices = _seedServices.OrderBy(s => s.Order).ToList();

        _logger.LogInformation("Starting execution of data seed services, total {Count} services", orderedServices.Count);

        foreach (var service in orderedServices)
        {
            try
            {
                _logger.LogInformation("Checking seed service: {Name} (Order: {Order})", service.Name, service.Order);

                var shouldExecute = await service.ShouldExecuteAsync();

                if (!shouldExecute)
                {
                    _logger.LogInformation("Seed service {Name} has already been executed or does not need to be executed, skipping", service.Name);
                    continue;
                }

                _logger.LogInformation("Starting execution of seed service: {Name}", service.Name);
                await service.SeedAsync();
                _logger.LogInformation("Seed service {Name} executed successfully", service.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seed service {Name} execution failed", service.Name);
                throw new Exception($"Seed service {service.Name} execution failed", ex);
            }
        }

        _logger.LogInformation("All data seed services executed successfully");
    }

    /// <summary>
    /// Executes the specified seed service by name
    /// </summary>
    public async Task ExecuteAsync(string serviceName)
    {
        var service = _seedServices.FirstOrDefault(s => s.Name == serviceName);

        if (service == null)
        {
            _logger.LogWarning("Seed service with name {ServiceName} not found", serviceName);
            return;
        }

        _logger.LogInformation("Starting execution of specified seed service: {Name}", service.Name);
        var shouldExecute = await service.ShouldExecuteAsync();

        if (!shouldExecute)
        {
            _logger.LogInformation("Seed service {Name} has already been executed or does not need to be executed, skipping", service.Name);
            return;
        }

        await service.SeedAsync();
        _logger.LogInformation("Seed service {Name} executed successfully", service.Name);
    }
}