using System.ComponentModel;

namespace StandardWeb.Common.Helpers;

/// <summary>
/// Provides utility methods for working with enumerations.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Retrieves the Description attribute value from an enum value.
    /// Useful for displaying user-friendly names for enum values.
    /// </summary>
    /// <param name="value">Enum value to get description for</param>
    /// <returns>Description from DescriptionAttribute or enum name if not found</returns>
    public static string? GetEnumDescription(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes == null) return null;

        return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : value.ToString();
    }

    /// <summary>
    /// Safely parses a string to enum value without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">Enum type to parse to</typeparam>
    /// <param name="value">String value to parse</param>
    /// <returns>Parsed enum value or null if parsing fails</returns>
    /// <exception cref="ArgumentException">Thrown when T is not an enum type</exception>
    public static T? TryToGetEnum<T>(string value) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enum");

        return Enum.TryParse<T>(value, out var result) ? result : null;
    }

    /// <summary>
    /// Parses a delimited string into a list of enum values.
    /// Example: "Active,Pending,Closed" -> [Status.Active, Status.Pending, Status.Closed]
    /// </summary>
    /// <typeparam name="T">Enum type to parse to</typeparam>
    /// <param name="enumString">Delimited string of enum names</param>
    /// <param name="separator">Delimiter character (default: comma)</param>
    /// <returns>List of parsed enum values or null if string is empty or no valid values found</returns>
    public static List<T>? GetListFromString<T>(string? enumString, char separator = ',') where T : struct
    {
        if (string.IsNullOrWhiteSpace(enumString)) return null;

        var enumNames = enumString.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var enumList = new List<T>();
        foreach (var name in enumNames)
        {
            var enumValue = TryToGetEnum<T>(name);
            if (enumValue != null)
            {
                enumList.Add(enumValue.Value);
            }
        }

        return enumList.Count > 0 ? enumList : null;
    }
}

