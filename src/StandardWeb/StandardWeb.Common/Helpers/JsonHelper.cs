using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace StandardWeb.Common.Helpers;

/// <summary>
/// Provides JSON serialization/deserialization and safe property extraction utilities.
/// Includes helpers for handling dynamic JSON structures and type-safe conversions.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Serializes an object to JSON string using System.Text.Json.
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <returns>JSON string representation</returns>
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    /// <summary>
    /// Deserializes JSON string to typed object with camelCase handling.
    /// </summary>
    /// <typeparam name="T">Target type for deserialization</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized object or null if parsing fails</returns>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        });
    }

    /// <summary>
    /// Safely extracts string property from JsonElement, returning empty string if not found.
    /// </summary>
    /// <param name="element">JSON element to search</param>
    /// <param name="propertyName">Property name to extract</param>
    /// <returns>Property value or empty string</returns>
    public static string GetStringProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind != JsonValueKind.Null)
        {
            return property.GetString() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Safely extracts double property, handling both numeric and string representations.
    /// </summary>
    /// <param name="element">JSON element to search</param>
    /// <param name="propertyName">Property name to extract</param>
    /// <returns>Property value or null if not found or invalid</returns>
    public static double? GetDoubleProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind != JsonValueKind.Null)
        {
            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetDouble(),
                JsonValueKind.String => double.TryParse(property.GetString(), out var result) ? result : null,
                _ => null
            };
        }
        return null;
    }

    /// <summary>
    /// Safely extracts integer property, handling both numeric and string representations.
    /// </summary>
    /// <param name="element">JSON element to search</param>
    /// <param name="propertyName">Property name to extract</param>
    /// <returns>Property value or null if not found or invalid</returns>
    public static int? GetIntProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind != JsonValueKind.Null)
        {
            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetInt32(),
                JsonValueKind.String => int.TryParse(property.GetString(), out var result) ? result : null,
                _ => null
            };
        }
        return null;
    }

    /// <summary>
    /// Safely extracts long integer property, handling both numeric and string representations.
    /// </summary>
    /// <param name="element">JSON element to search</param>
    /// <param name="propertyName">Property name to extract</param>
    /// <returns>Property value or null if not found or invalid</returns>
    public static long? GetLongProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind != JsonValueKind.Null)
        {
            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetInt64(),
                JsonValueKind.String => long.TryParse(property.GetString(), out var result) ? result : null,
                _ => null
            };
        }
        return null;
    }

    /// <summary>
    /// Safely extracts boolean property, handling both boolean and string representations.
    /// </summary>
    /// <param name="element">JSON element to search</param>
    /// <param name="propertyName">Property name to extract</param>
    /// <returns>Property value or null if not found or invalid</returns>
    public static bool? GetBoolProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind != JsonValueKind.Null)
        {
            return property.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(property.GetString(), out var result) ? result : null,
                _ => null
            };
        }
        return null;
    }

    /// <summary>
    /// Safely parses date property and returns as yyyy-MM-dd format string.
    /// </summary>
    /// <param name="element">JSON element to search</param>
    /// <param name="propertyName">Property name to extract</param>
    /// <returns>Formatted date string or empty string if invalid</returns>
    public static string ParseDateProperty(JsonElement element, string propertyName)
    {
        var dateString = GetStringProperty(element, propertyName);
        if (!string.IsNullOrEmpty(dateString) && DateTime.TryParse(dateString, out var date))
        {
            return date.ToString("yyyy-MM-dd");
        }
        return string.Empty;
    }

    /// <summary>
    /// Safely parses date property and returns as DateTime object.
    /// </summary>
    /// <param name="element">JSON element to search</param>
    /// <param name="propertyName">Property name to extract</param>
    /// <returns>DateTime value or null if invalid</returns>
    public static DateTime? ParseDateTimeProperty(JsonElement element, string propertyName)
    {
        var dateString = GetStringProperty(element, propertyName);
        if (!string.IsNullOrEmpty(dateString) && DateTime.TryParse(dateString, out var date))
        {
            return date;
        }
        return null;
    }

    /// <summary>
    /// Checks if JSON element has a valid (non-null) property.
    /// </summary>
    /// <param name="element">JSON element to check</param>
    /// <param name="propertyName">Property name to verify</param>
    /// <returns>True if property exists and is not null</returns>
    public static bool HasValidProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind != JsonValueKind.Null;
    }

    /// <summary>
    /// Safely navigates nested JSON properties using a property path.
    /// Example: GetNestedProperty(element, "user", "profile", "name")
    /// </summary>
    /// <param name="element">Root JSON element</param>
    /// <param name="propertyPath">Sequence of property names to navigate</param>
    /// <returns>Final nested element or null if any property in path is missing</returns>
    public static JsonElement? GetNestedProperty(JsonElement element, params string[] propertyPath)
    {
        var current = element;

        foreach (var property in propertyPath)
        {
            if (!current.TryGetProperty(property, out current) ||
                current.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
        }

        return current;
    }

    /// <summary>
    /// Parses string to double, removing common special characters (+, %).
    /// Useful for parsing percentages and formatted numbers.
    /// </summary>
    /// <param name="value">String value to parse</param>
    /// <returns>Parsed double value or 0 if invalid</returns>
    public static double ParseStringToDouble(string? value)
    {
        if (string.IsNullOrEmpty(value)) return 0;

        var cleanValue = value.Replace("+", "").Replace("%", "");
        return double.TryParse(cleanValue, out var result) ? result : 0;
    }

    /// <summary>
    /// Generic property value extractor with type-safe conversion and default value.
    /// </summary>
    /// <typeparam name="T">Target type for conversion</typeparam>
    /// <param name="element">JSON element to search</param>
    /// <param name="propertyName">Property name to extract</param>
    /// <param name="defaultValue">Default value if property not found or conversion fails</param>
    /// <returns>Converted property value or default</returns>
    public static T GetPropertyValue<T>(JsonElement element, string propertyName, T defaultValue = default!)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return defaultValue;
        }

        return typeof(T) switch
        {
            Type t when t == typeof(string) => (T)(object)(property.GetString() ?? string.Empty),
            Type t when t == typeof(int) => (T)(object)property.GetInt32(),
            Type t when t == typeof(long) => (T)(object)property.GetInt64(),
            Type t when t == typeof(double) => (T)(object)property.GetDouble(),
            Type t when t == typeof(bool) => (T)(object)property.GetBoolean(),
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts JSON array to DataTable for legacy code or reporting scenarios.
    /// Each JSON object becomes a row, with properties as columns.
    /// </summary>
    /// <param name="jsonArray">JSON array element</param>
    /// <returns>DataTable with converted data or null if invalid</returns>
    public static DataTable? ConvertJsonArrayToDataTable(JsonElement jsonArray)
    {
        if (jsonArray.ValueKind != JsonValueKind.Array || jsonArray.GetArrayLength() == 0)
            return null;

        var dataTable = new DataTable();

        try
        {
            foreach (var item in jsonArray.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;

                var row = dataTable.NewRow();
                foreach (var property in item.EnumerateObject())
                {
                    if (!dataTable.Columns.Contains(property.Name))
                    {
                        dataTable.Columns.Add(property.Name, typeof(string));
                    }
                    var value = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString(),
                        JsonValueKind.Number => property.Value.GetDouble().ToString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null => null,
                        _ => property.Value.GetRawText()
                    };

                    row[property.Name] = value ?? string.Empty;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting JSON array to DataTable: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Converts JSON array to strongly-typed List with custom DateTime handling.
    /// Uses custom converters for flexible date format support.
    /// </summary>
    /// <typeparam name="T">Target type for list items</typeparam>
    /// <param name="jsonArray">JSON array element</param>
    /// <param name="logger">Optional logger for error tracking</param>
    /// <returns>List of typed objects or null if conversion fails</returns>
    public static List<T>? ConvertJsonToList<T>(JsonElement jsonArray, ILogger? logger = null)
    {
        if (jsonArray.ValueKind != JsonValueKind.Array || jsonArray.GetArrayLength() == 0)
            return null;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            options.Converters.Add(new CustomDateTimeConverter());
            options.Converters.Add(new CustomNullableDateTimeConverter());

            var result = new List<T>();
            foreach (var item in jsonArray.EnumerateArray())
            {
                var obj = JsonSerializer.Deserialize<T>(item.GetRawText(), options);
                if (obj != null)
                {
                    result.Add(obj);
                }
            }

            return result.Count > 0 ? result : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting JSON array to List<{typeof(T).Name}>: {ex.Message}");
            logger?.LogError(ex, "Error converting JSON array to List<{TypeName}>", typeof(T).Name);

            return null;
        }
    }

    /// <summary>
    /// Custom DateTime converter supporting "yyyy-MM-dd HH:mm:ss" and standard formats.
    /// </summary>
    private class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
                return default;

            // Try parsing with flexible format support
            if (DateTime.TryParse(dateString, out var date))
                return date;

            throw new JsonException($"Unable to parse DateTime: {dateString}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    /// <summary>
    /// Custom nullable DateTime converter with same format support as CustomDateTimeConverter.
    /// </summary>
    private class CustomNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            if (DateTime.TryParse(dateString, out var date))
                return date;

            throw new JsonException($"Unable to parse DateTime?: {dateString}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            else
                writer.WriteNullValue();
        }
    }
}
