using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

public static class AttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        // String methods
        public string? GetNullableString(string key)
        {
            return attributes.TryGetValue(key, out var value) ? value.S : null;
        }

        public string GetString(string key)
        {
            return attributes.TryGetValue(key, out var value) ? value.S : string.Empty;
        }

        // Boolean methods
        public bool GetBool(string key)
        {
            return attributes.TryGetValue(key, out var value) && (value.BOOL ?? false);
        }

        public bool? GetNullableBool(string key)
        {
            return attributes.TryGetValue(key, out var value) ? value.BOOL : null;
        }

        // Integer methods
        public int GetInt(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? int.TryParse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0
                : 0;
        }

        public int? GetNullableInt(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? int.TryParse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null
                : null;
        }

        public long GetLong(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? long.TryParse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0
                : 0;
        }

        public long? GetNullableLong(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? long.TryParse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null
                : null;
        }

        // Floating point methods
        public float GetFloat(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? float.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0f
                : 0f;
        }

        public float? GetNullableFloat(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? float.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null
                : null;
        }

        public double GetDouble(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? double.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0.0
                : 0.0;
        }

        public double? GetNullableDouble(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? double.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null
                : null;
        }

        public decimal GetDecimal(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? decimal.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0m
                : 0m;
        }

        public decimal? GetNullableDecimal(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? decimal.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null
                : null;
        }

        // Guid methods
        public Guid GetGuid(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? Guid.TryParse(value.S, out var id) ? id : Guid.Empty
                : Guid.Empty;
        }

        public Guid? GetNullableGuid(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? Guid.TryParse(value.S, out var id) ? id : null
                : null;
        }
    }
}