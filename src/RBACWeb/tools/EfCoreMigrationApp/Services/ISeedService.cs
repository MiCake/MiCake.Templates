namespace EfCoreMigrationApp.Services;

/// <summary>
/// Seed service interface
/// </summary>
public interface ISeedService
{
    /// <summary>
    /// The execution order of the seed service (lower numbers execute first)
    /// </summary>
    int Order { get; }

    /// <summary>
    /// The name of the seed service
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the data seeding operation
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// Checks whether execution is needed (to avoid duplicate execution)
    /// </summary>
    Task<bool> ShouldExecuteAsync();
}