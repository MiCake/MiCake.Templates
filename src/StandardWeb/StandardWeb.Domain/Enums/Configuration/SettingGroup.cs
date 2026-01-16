namespace StandardWeb.Domain.Enums.Configuration;

/// <summary>
/// Defines logical grouping of application settings.
/// Provides strong typing and prevents arbitrary group creation.
/// </summary>
public enum SettingGroup
{
    /// <summary>Email service settings (SMTP, port, authentication)</summary>
    Email = 1,

    /// <summary>SMS service settings (provider, API keys, templates)</summary>
    Sms = 2,

    /// <summary>Payment gateway settings (API credentials, callbacks)</summary>
    Payment = 3,

    /// <summary>General system settings (maintenance mode, feature flags)</summary>
    System = 4,

    /// <summary>Security and authentication settings (password policy, lockout)</summary>
    Security = 5,

    /// <summary>File storage settings (upload limits, allowed types, paths)</summary>
    Storage = 6,

    /// <summary>Notification settings (push, email, in-app)</summary>
    Notification = 7,

    /// <summary>Integration with external services (APIs, webhooks)</summary>
    Integration = 8
}
