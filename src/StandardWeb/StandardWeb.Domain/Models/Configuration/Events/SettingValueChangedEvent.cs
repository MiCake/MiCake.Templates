using StandardWeb.Domain.Enums.Configuration;

namespace StandardWeb.Domain.Models.Configuration.Events;

/// <summary>
/// Domain event raised when a setting value is changed.
/// Enables cache invalidation, audit logging, and notifications.
/// </summary>
/// <param name="SettingId">ID of the changed setting</param>
/// <param name="SettingGroup">Group the setting belongs to</param>
/// <param name="Key">Setting key</param>
/// <param name="OldValue">Previous value (encrypted if applicable)</param>
/// <param name="NewValue">New value (encrypted if applicable)</param>
/// <param name="ChangedBy">User who made the change</param>
/// <param name="ChangedAt">Timestamp of the change</param>
public record SettingValueChangedEvent(
    long SettingId,
    SettingGroup SettingGroup,
    string Key,
    string OldValue,
    string NewValue,
    long? ChangedBy,
    DateTime ChangedAt) : IDomainEvent;
