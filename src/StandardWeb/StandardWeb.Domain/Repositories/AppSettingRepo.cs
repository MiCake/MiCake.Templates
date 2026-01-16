using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using StandardWeb.Domain.Enums.Configuration;
using StandardWeb.Domain.Models.Configuration;

namespace StandardWeb.Domain.Repositories;

/// <summary>
/// Repository implementation for AppSetting aggregate.
/// </summary>
public class AppSettingRepo : BaseRepository<AppSetting>, IAppSettingRepo
{
    public AppSettingRepo(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    public async Task<AppSetting?> GetByKeyAsync(SettingGroup settingGroup, string key, bool needTracking = true, CancellationToken ct = default)
    {
        return await GetDbSet(needTracking).
            FirstOrDefaultAsync(s => s.SettingGroup == settingGroup && s.Key == key, ct);
    }

    public async Task<IReadOnlyList<AppSetting>> GetByGroupAsync(SettingGroup settingGroup, bool needTracking = true, CancellationToken ct = default)
    {
        return await GetDbSet(needTracking)
            .Where(s => s.SettingGroup == settingGroup)
            .OrderBy(s => s.Key)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, string>> GetGroupAsDictionaryAsync(SettingGroup settingGroup, CancellationToken ct = default)
    {
        return await GetDbSet(needTracking: false)
            .Where(s => s.SettingGroup == settingGroup)
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct);
    }

    public async Task<bool> ExistsAsync(SettingGroup settingGroup, string key, CancellationToken ct = default)
    {
        return await GetDbSet(needTracking: false)
            .AnyAsync(s => s.SettingGroup == settingGroup && s.Key == key, ct);
    }
}
