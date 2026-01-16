namespace StandardWeb.Domain.Enums.Configuration;

/// <summary>
/// Specifies the data type of a setting value for validation and parsing.
/// </summary>
public enum SettingDataType
{
    /// <summary>String value</summary>
    String = 1,

    /// <summary>Integer value</summary>
    Integer = 2,

    /// <summary>Boolean value (true/false)</summary>
    Boolean = 3,

    /// <summary>Decimal value (for currency, percentages)</summary>
    Decimal = 4,
}
