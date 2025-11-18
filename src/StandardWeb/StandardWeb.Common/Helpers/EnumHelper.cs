using System.ComponentModel;

namespace StandardWeb.Common.Helpers;

public static class EnumHelper
{
    public static string? GetEnumDescription(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes == null) return null;

        return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : value.ToString();
    }

    public static T? TryToGetEnum<T>(string value) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enum");

        return Enum.TryParse<T>(value, out var result) ? result : null;
    }

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

