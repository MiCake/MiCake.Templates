using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using StandardWeb.Domain.Enums.Configuration;
using StandardWeb.Domain.Models.Configuration.Events;

namespace StandardWeb.Domain.Models.Configuration;

/// <summary>
/// Aggregate root for dynamic application configuration settings.
/// Supports type-safe setting groups, encryption, validation patterns, and audit tracking.
/// </summary>
public class AppSetting : AuditAggregateRoot
{
    /// <summary>
    /// Logical group this setting belongs to (Email, Sms, Payment, etc.)
    /// </summary>
    [Required]
    public SettingGroup SettingGroup { get; private set; }

    /// <summary>
    /// Unique key within the setting group (e.g., "SmtpServer", "ApiKey")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Key { get; private set; } = null!;

    /// <summary>
    /// Setting value (already encrypted if IsEncrypted=true).
    /// Application layer handles encryption/decryption.
    /// </summary>
    [Required]
    public string Value { get; private set; } = null!;

    /// <summary>
    /// Data type of the value for validation and parsing
    /// </summary>
    [Required]
    public SettingDataType DataType { get; private set; }

    /// <summary>
    /// Human-readable description of this setting's purpose
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates if the value is encrypted.
    /// Actual encryption is handled by Application layer.
    /// </summary>
    public bool IsEncrypted { get; private set; }

    /// <summary>
    /// Optional regex pattern for value validation.
    /// Enforces format constraints (e.g., email format, port range).
    /// </summary>
    [MaxLength(500)]
    public string? ValidationPattern { get; private set; }

    // Private constructor for EF Core
    private AppSetting() { }

    /// <summary>
    /// Factory method to create a new AppSetting.
    /// Validates all parameters and optional validation pattern.
    /// </summary>
    /// <param name="settingGroup">Setting group (Email, Sms, etc.)</param>
    /// <param name="key">Unique key within the group</param>
    /// <param name="value">Value (already encrypted if isEncrypted=true)</param>
    /// <param name="dataType">Data type for parsing</param>
    /// <param name="isEncrypted">Whether value is encrypted</param>
    /// <param name="description">Optional description</param>
    /// <param name="validationPattern">Optional regex pattern for validation</param>
    /// <returns>New AppSetting instance</returns>
    /// <exception cref="ArgumentException">If parameters are invalid</exception>
    public static AppSetting Create(
        SettingGroup settingGroup,
        string key,
        string value,
        SettingDataType dataType,
        bool isEncrypted = false,
        string? description = null,
        string? validationPattern = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Setting key cannot be empty", nameof(key));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Setting value cannot be empty", nameof(value));

        if (key.Length > 100)
            throw new ArgumentException("Setting key cannot exceed 100 characters", nameof(key));

        if (description?.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        if (validationPattern?.Length > 500)
            throw new ArgumentException("Validation pattern cannot exceed 500 characters", nameof(validationPattern));

        var setting = new AppSetting
        {
            SettingGroup = settingGroup,
            Key = key,
            Value = value,
            DataType = dataType,
            IsEncrypted = isEncrypted,
            Description = description,
            ValidationPattern = validationPattern
        };

        // Validate pattern if provided (validation should be done in Application layer before creation)
        // This is a safety check
        if (!string.IsNullOrEmpty(validationPattern) && !setting.ValidateValue(value))
        {
            throw new ArgumentException(
                $"Value does not match validation pattern: {validationPattern}",
                nameof(value));
        }

        return setting;
    }

    /// <summary>
    /// Updates the setting value.
    /// Note: If encryption is needed, value should already be encrypted by Application layer.
    /// </summary>
    /// <param name="newValue">New value (encrypted if IsEncrypted=true)</param>
    /// <exception cref="ArgumentException">If value is invalid or doesn't match pattern</exception>
    public void UpdateValue(string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
            throw new ArgumentException("Setting value cannot be empty", nameof(newValue));

        // Validate pattern if specified
        if (!ValidateValue(newValue))
        {
            throw new ArgumentException(
                $"Value does not match validation pattern: {ValidationPattern}",
                nameof(newValue));
        }

        Value = newValue;

        // TODO: Raise domain event for cache invalidation and audit
        // Will be implemented when domain event infrastructure is ready
    }

    /// <summary>
    /// Updates the setting description.
    /// </summary>
    public void UpdateDescription(string? description)
    {
        if (description?.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        Description = description;
    }

    /// <summary>
    /// Updates the validation pattern.
    /// Validates current value against new pattern.
    /// </summary>
    public void UpdateValidationPattern(string? pattern)
    {
        if (pattern?.Length > 500)
            throw new ArgumentException("Validation pattern cannot exceed 500 characters", nameof(pattern));

        // Validate current value against new pattern
        if (!string.IsNullOrEmpty(pattern) && !Regex.IsMatch(Value, pattern))
        {
            throw new ArgumentException(
                "Current value does not match the new validation pattern",
                nameof(pattern));
        }

        ValidationPattern = pattern;
    }

    /// <summary>
    /// Validates a value against the ValidationPattern.
    /// Returns true if no pattern is set or if value matches pattern.
    /// </summary>
    public bool ValidateValue(string value)
    {
        if (string.IsNullOrEmpty(ValidationPattern))
            return true;

        try
        {
            return Regex.IsMatch(value, ValidationPattern);
        }
        catch (ArgumentException)
        {
            // Invalid regex pattern
            return false;
        }
    }

    /// <summary>
    /// Checks if the current value matches the validation pattern.
    /// </summary>
    public bool IsValidFormat() => ValidateValue(Value);
}
