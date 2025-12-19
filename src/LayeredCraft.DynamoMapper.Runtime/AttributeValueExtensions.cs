namespace LayeredCraft.DynamoMapper.Runtime;

using Amazon.DynamoDBv2.Model;

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
    }
}