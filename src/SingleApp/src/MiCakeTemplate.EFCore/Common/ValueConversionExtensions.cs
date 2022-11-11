using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

/*
 * Tips: This folder is used to store some common base classes
 */

namespace MiCakeTemplate.EFCore.Common
{
    // This class is used to convert object to string when efcore save data.
    internal static class ValueConversionExtensions
    {
        private static readonly JsonSerializerOptions conversionJsonOptions = new(JsonSerializerDefaults.Web);

        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
        {
            ValueConverter<T, string> converter = new(
                v => JsonSerializer.Serialize(v, typeof(T), conversionJsonOptions),
                v => JsonSerializer.Deserialize<T>(v, conversionJsonOptions)!);

            ValueComparer<T> comparer = new(
                (l, r) => JsonSerializer.Serialize(l, typeof(T), conversionJsonOptions) == JsonSerializer.Serialize(r, typeof(T), conversionJsonOptions),
                v => v == null ? 0 : JsonSerializer.Serialize(v, typeof(T), conversionJsonOptions).GetHashCode(),
                v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, typeof(T), conversionJsonOptions), conversionJsonOptions)!);

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }
    }
}
