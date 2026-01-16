namespace StandardWeb.Application.ErrorCodes;

/// <summary>
/// Error codes for Configuration module.
/// Module code: "05"
/// </summary>
public static class ConfigurationErrorCodes
{
    public const string SettingNotFound = "1101";

    public const string SettingAlreadyExists = "1102";

    public const string InvalidDataType = "1103";

    public const string InvalidValue = "1104";

    public const string GroupNotFound = "1105";

    public const string CannotDeleteRequiredSetting = "1106";

    public const string DecryptionFailed = "1107";

    public const string ValidationPatternMismatch = "1108";

    public const string EncryptionFailed = "1109";

    public const string InvalidRegexPattern = "1110";
}
