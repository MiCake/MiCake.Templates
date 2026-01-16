using StandardWeb.Domain.Enums.Configuration;
using StandardWeb.Domain.Models.Configuration;

namespace StandardWeb.Domain.Repositories;

/// <summary>
/// Repository interface for AppSetting aggregate.
/// Provides specialized queries for configuration management.
/// </summary>
public interface IAppSettingRepo : IRepositoryHasPagingQuery<AppSetting, long>
{
    /// <summary>
    /// Gets a setting by its group and key.
    /// </summary>
    /// <param name="settingGroup">Setting group (Email, Sms, etc.)</param>
    /// <param name="key">Setting key</param>
    /// <param name="needTracking">Enable change tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppSetting if found, null otherwise</returns>
    Task<AppSetting?> GetByKeyAsync(
        SettingGroup settingGroup,
        string key,
        bool needTracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all settings in a specific group.
    /// </summary>
    /// <param name="settingGroup">Setting group</param>
    /// <param name="needTracking">Enable change tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of settings in the group</returns>
    Task<IReadOnlyList<AppSetting>> GetByGroupAsync(
        SettingGroup settingGroup,
        bool needTracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets settings in a group as a dictionary (key -> value).
    /// Values are NOT decrypted - use Application layer for decryption.
    /// </summary>
    /// <param name="settingGroup">Setting group</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of key-value pairs</returns>
    Task<Dictionary<string, string>> GetGroupAsDictionaryAsync(
        SettingGroup settingGroup,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a setting exists with the given group and key.
    /// </summary>
    /// <param name="settingGroup">Setting group</param>
    /// <param name="key">Setting key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if setting exists</returns>
    Task<bool> ExistsAsync(
        SettingGroup settingGroup,
        string key,
        CancellationToken cancellationToken = default);
}
