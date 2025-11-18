using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace StandardWeb.Common.Helpers;

public static class JsonHelper
{
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            // ignore null
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        });
    }

    /// <summary>
    /// 安全获取字符串属性
    /// </summary>
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
    /// 安全获取双精度属性
    /// </summary>
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
    /// 安全获取整数属性
    /// </summary>
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
    /// 安全获取长整数属性
    /// </summary>
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
    /// 安全获取布尔属性
    /// </summary>
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
    /// 安全解析日期属性
    /// </summary>
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
    /// 安全解析日期时间属性
    /// </summary>
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
    /// 检查JSON元素是否存在且不为null
    /// </summary>
    public static bool HasValidProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind != JsonValueKind.Null;
    }

    /// <summary>
    /// 安全获取嵌套属性
    /// </summary>
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
    /// 解析字符串为double，处理特殊字符
    /// </summary>
    public static double ParseStringToDouble(string? value)
    {
        if (string.IsNullOrEmpty(value)) return 0;

        var cleanValue = value.Replace("+", "").Replace("%", "");
        return double.TryParse(cleanValue, out var result) ? result : 0;
    }

    /// <summary>
    /// 安全获取属性值并转换为指定类型
    /// </summary>
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
    /// Convert JsonElement array to List of typed objects
    /// </summary>
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
    /// Custom DateTime converter to handle formats like "yyyy-MM-dd HH:mm:ss"
    /// </summary>
    private class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
                return default;

            // Try parsing the custom format first, then fall back to standard parsing
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
    /// Custom Nullable DateTime converter
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
