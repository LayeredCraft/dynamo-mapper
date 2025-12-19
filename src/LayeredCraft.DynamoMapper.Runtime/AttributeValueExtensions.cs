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
    }
}