using System.Globalization;
using System.IO;
using DynamoMapper.Runtime;

namespace Amazon.DynamoDBv2.Model;

/// <summary>
/// Extension methods for collection types (Lists, Maps, Sets).
/// </summary>
public static class CollectionAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        // ==================== LIST OPERATIONS ====================

        /// <summary>Gets a list of elements from the attribute dictionary.</summary>
        public List<T> GetList<T>(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.L
        )
        {
            var attributeValue = attributes.GetValue(key, requiredness);
            if (attributeValue.NULL is true)
                return [];

            var list = attributeValue.L ?? [];
            return list.Select(Dictionary<string, AttributeValue>.ConvertFromAttributeValue<T>).ToList();
        }

        /// <summary>Gets a nullable list from the attribute dictionary.</summary>
        public List<T>? GetNullableList<T>(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.L
        )
        {
            var attributeValue = attributes.GetNullableValue(key, requiredness);
            if (attributeValue.NULL is true)
                return null;

            var list = attributeValue.L ?? [];
            return list.Select(Dictionary<string, AttributeValue>.ConvertFromAttributeValue<T>).ToList();
        }

        /// <summary>Sets a list in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetList<T>(
            string key, IEnumerable<T>? value, bool omitEmptyStrings = false,
            bool omitNullStrings = true, DynamoKind kind = DynamoKind.L
        )
        {
            if (value is null && omitNullStrings)
                return attributes;

            if (value is null)
            {
                attributes[key] = new AttributeValue { NULL = true };
                return attributes;
            }

            var list =
                value.Select(Dictionary<string, AttributeValue>.ConvertToAttributeValue).ToList();

            // Empty lists ARE allowed in DynamoDB - respect omitEmptyStrings flag
            if (list.Count == 0 && omitEmptyStrings)
                return attributes;

            attributes[key] = new AttributeValue { L = list };
            return attributes;
        }

        // ==================== MAP OPERATIONS ====================

        /// <summary>Gets a map from the attribute dictionary.</summary>
        public Dictionary<string, T> GetMap<T>(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.M
        )
        {
            var attributeValue = attributes.GetValue(key, requiredness);
            if (attributeValue.NULL is true)
                return new Dictionary<string, T>();

            var map = attributeValue.M ?? new Dictionary<string, AttributeValue>();
            return map.ToDictionary(kvp => kvp.Key, kvp => Dictionary<string, AttributeValue>.ConvertFromAttributeValue<T>(kvp.Value));
        }

        /// <summary>Gets a nullable map from the attribute dictionary.</summary>
        public Dictionary<string, T>? GetNullableMap<T>(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.M
        )
        {
            var attributeValue = attributes.GetNullableValue(key, requiredness);
            if (attributeValue.NULL is true)
                return null;

            var map = attributeValue.M ?? new Dictionary<string, AttributeValue>();
            return map.ToDictionary(kvp => kvp.Key, kvp => Dictionary<string, AttributeValue>.ConvertFromAttributeValue<T>(kvp.Value));
        }

        /// <summary>Sets a map in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetMap<T>(
            string key, IDictionary<string, T>? value, bool omitEmptyStrings = false,
            bool omitNullStrings = true, DynamoKind kind = DynamoKind.M
        )
        {
            if (value is null && omitNullStrings)
                return attributes;

            if (value is null)
            {
                attributes[key] = new AttributeValue { NULL = true };
                return attributes;
            }

            var map =
                value.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Dictionary<string, AttributeValue>.ConvertToAttributeValue(kvp.Value)
                );

            // Empty maps ARE allowed in DynamoDB - respect omitEmptyStrings flag
            if (map.Count == 0 && omitEmptyStrings)
                return attributes;

            attributes[key] = new AttributeValue { M = map };
            return attributes;
        }

        // ==================== STRING SET OPERATIONS ====================

        /// <summary>Gets a string set from the attribute dictionary.</summary>
        public HashSet<string> GetStringSet(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.SS
        )
        {
            var attributeValue = attributes.GetValue(key, requiredness);
            if (attributeValue.NULL is true)
                return [];

            var ss = attributeValue.SS ?? [];
            return [..ss];
        }

        /// <summary>Gets a nullable string set.</summary>
        public HashSet<string>? GetNullableStringSet(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.SS
        )
        {
            var attributeValue = attributes.GetNullableValue(key, requiredness);
            if (attributeValue.NULL is true)
                return null;

            var ss = attributeValue.SS ?? [];
            return [..ss];
        }

        /// <summary>Sets a string set in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetStringSet(
            string key, IEnumerable<string>? value, bool omitEmptyStrings = false,
            bool omitNullStrings = true, DynamoKind kind = DynamoKind.SS
        )
        {
            if (value is null && omitNullStrings)
                return attributes;

            var set = value?.Where(s => s != null).Distinct().ToList() ?? new List<string>();

            // DynamoDB does NOT allow empty sets - always omit
            // Note: NULL sets are handled above, this is for genuinely empty collections
            if (set.Count == 0)
                return attributes;

            attributes[key] = new AttributeValue { SS = set };
            return attributes;
        }

        // ==================== NUMBER SET OPERATIONS ====================

        /// <summary>Gets a number set from the attribute dictionary.</summary>
        public HashSet<T> GetNumberSet<T>(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.NS
        ) where T : struct
        {
            var attributeValue = attributes.GetValue(key, requiredness);
            if (attributeValue.NULL is true)
                return [];

            var ns = attributeValue.NS ?? [];
            return [..ns.Select(Dictionary<string, AttributeValue>.ParseNumber<T>)];
        }

        /// <summary>Gets a nullable number set.</summary>
        public HashSet<T>? GetNullableNumberSet<T>(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.NS
        ) where T : struct
        {
            var attributeValue = attributes.GetNullableValue(key, requiredness);
            if (attributeValue.NULL is true)
                return null;

            var ns = attributeValue.NS ?? [];
            return [..ns.Select(Dictionary<string, AttributeValue>.ParseNumber<T>)];
        }

        /// <summary>Sets a number set in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetNumberSet<T>(
            string key, IEnumerable<T>? value, bool omitEmptyStrings = false,
            bool omitNullStrings = true, DynamoKind kind = DynamoKind.NS
        ) where T : struct
        {
            if (value is null && omitNullStrings)
                return attributes;

            var set =
                value?.Distinct()
                    .Select(Dictionary<string, AttributeValue>.FormatNumber)
                    .ToList() ?? [];

            // DynamoDB does NOT allow empty sets - always omit
            if (set.Count == 0)
                return attributes;

            attributes[key] = new AttributeValue { NS = set };
            return attributes;
        }

        // ==================== BINARY SET OPERATIONS ====================

        /// <summary>Gets a binary set from the attribute dictionary.</summary>
        public HashSet<byte[]> GetBinarySet(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BS
        )
        {
            var attributeValue = attributes.GetValue(key, requiredness);
            if (attributeValue.NULL is true)
                return new HashSet<byte[]>(ByteArrayComparer.Instance);

            var bs = attributeValue.BS ?? [];
            return new HashSet<byte[]>(
                bs.Select(stream => stream.ToArray()),
                ByteArrayComparer.Instance
            );
        }

        /// <summary>Gets a nullable binary set.</summary>
        public HashSet<byte[]>? GetNullableBinarySet(
            string key, Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BS
        )
        {
            var attributeValue = attributes.GetNullableValue(key, requiredness);
            if (attributeValue.NULL is true)
                return null;

            var bs = attributeValue.BS ?? [];
            return new HashSet<byte[]>(
                bs.Select(stream => stream.ToArray()),
                ByteArrayComparer.Instance
            );
        }

        /// <summary>Sets a binary set in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetBinarySet(
            string key, IEnumerable<byte[]>? value, bool omitEmptyStrings = false,
            bool omitNullStrings = true, DynamoKind kind = DynamoKind.BS
        )
        {
            if (value is null && omitNullStrings)
                return attributes;

            var set = value?
                .Where(bytes => bytes is not null)
                .Distinct(ByteArrayComparer.Instance)
                .Select(bytes => new MemoryStream(bytes!))
                .ToList() ?? new List<MemoryStream>();

            // DynamoDB does NOT allow empty sets - always omit
            if (set.Count == 0)
                return attributes;

            attributes[key] = new AttributeValue { BS = set };
            return attributes;
        }

        // ==================== HELPER METHODS ====================

        /// <summary>
        /// Converts an AttributeValue to a strongly-typed value.
        /// Handles NULL AttributeValues for nullable elements.
        /// </summary>
        private static T ConvertFromAttributeValue<T>(AttributeValue av)
        {
            // Handle NULL AttributeValues (for nullable elements in collections)
            if (av.NULL is true)
                return default!;

            var type = typeof(T);

            // Handle Nullable<T>
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // String
            if (underlyingType == typeof(string))
                return (T)(object?)av.GetNullableString(DynamoKind.S)!;

            // Boolean
            if (underlyingType == typeof(bool))
                return (T)(object)av.GetBool(DynamoKind.BOOL);

            // Numeric types
            if (underlyingType == typeof(int))
                return (T)(object)int.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(long))
                return (T)(object)long.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(float))
                return (T)(object)float.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(double))
                return (T)(object)double.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(decimal))
                return (T)(object)decimal.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(byte))
                return (T)(object)byte.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(short))
                return (T)(object)short.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);

            // DateTime
            if (underlyingType == typeof(DateTime))
                return (T)(object)DateTime.Parse(
                    av.GetString(DynamoKind.S),
                    CultureInfo.InvariantCulture
                );

            // DateTimeOffset
            if (underlyingType == typeof(DateTimeOffset))
                return (T)(object)DateTimeOffset.Parse(
                    av.GetString(DynamoKind.S),
                    CultureInfo.InvariantCulture
                );

            // TimeSpan
            if (underlyingType == typeof(TimeSpan))
                return (T)(object)TimeSpan.Parse(
                    av.GetString(DynamoKind.S),
                    CultureInfo.InvariantCulture
                );

            // Guid
            if (underlyingType == typeof(Guid))
                return (T)(object)Guid.Parse(av.GetString(DynamoKind.S));

            // Enum
            if (underlyingType.IsEnum)
                return (T)Enum.Parse(underlyingType, av.GetString(DynamoKind.S));

            // byte[]
            if (underlyingType == typeof(byte[]))
                return (T)(object)(av.B?.ToArray() ?? []);

            throw new NotSupportedException(
                $"Type {typeof(T)} is not supported as a collection element"
            );
        }

        /// <summary>
        /// Converts a strongly-typed value to an AttributeValue.
        /// Handles null values by creating NULL AttributeValues.
        /// </summary>
        private static AttributeValue ConvertToAttributeValue<T>(T? value)
        {
            // Handle null values (for nullable elements in collections)
            if (value is null)
                return new AttributeValue { NULL = true };

            var type = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // String
            if (underlyingType == typeof(string))
                return ((string?)(object)value).ToAttributeValue(DynamoKind.S);

            // Boolean
            if (underlyingType == typeof(bool))
                return ((bool?)(object)value).ToAttributeValue(DynamoKind.BOOL);

            // Numeric types
            if (underlyingType == typeof(int))
                return ((int)(object)value)
                    .ToString(CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.N);
            if (underlyingType == typeof(long))
                return ((long)(object)value)
                    .ToString(CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.N);
            if (underlyingType == typeof(float))
                return ((float)(object)value)
                    .ToString(CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.N);
            if (underlyingType == typeof(double))
                return ((double)(object)value)
                    .ToString(CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.N);
            if (underlyingType == typeof(decimal))
                return ((decimal)(object)value)
                    .ToString(CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.N);
            if (underlyingType == typeof(byte))
                return ((byte)(object)value)
                    .ToString(CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.N);
            if (underlyingType == typeof(short))
                return ((short)(object)value)
                    .ToString(CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.N);

            // DateTime
            if (underlyingType == typeof(DateTime))
                return ((DateTime)(object)value)
                    .ToString("O", CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.S);

            // DateTimeOffset
            if (underlyingType == typeof(DateTimeOffset))
                return ((DateTimeOffset)(object)value)
                    .ToString("O", CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.S);

            // TimeSpan
            if (underlyingType == typeof(TimeSpan))
                return ((TimeSpan)(object)value)
                    .ToString("c", CultureInfo.InvariantCulture)
                    .ToAttributeValue(DynamoKind.S);

            // Guid
            if (underlyingType == typeof(Guid))
                return ((Guid)(object)value)
                    .ToString()
                    .ToAttributeValue(DynamoKind.S);

            // byte[]
            if (underlyingType == typeof(byte[]))
                return new AttributeValue { B = new MemoryStream((byte[])(object)value) };

            // Enum
            return underlyingType.IsEnum
                ? (value.ToString() ?? string.Empty).ToAttributeValue(DynamoKind.S)
                : throw new NotSupportedException(
                    $"Type {typeof(T)} is not supported as a collection element"
                );
        }

        /// <summary>
        /// Parses a number from DynamoDB's string representation.
        /// Uses InvariantCulture for culture-safe parsing.
        /// </summary>
        private static T ParseNumber<T>(string s) where T : struct
        {
            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(s, CultureInfo.InvariantCulture);
            if (typeof(T) == typeof(long))
                return (T)(object)long.Parse(s, CultureInfo.InvariantCulture);
            if (typeof(T) == typeof(float))
                return (T)(object)float.Parse(s, CultureInfo.InvariantCulture);
            if (typeof(T) == typeof(double))
                return (T)(object)double.Parse(s, CultureInfo.InvariantCulture);
            if (typeof(T) == typeof(decimal))
                return (T)(object)decimal.Parse(s, CultureInfo.InvariantCulture);
            if (typeof(T) == typeof(byte))
                return (T)(object)byte.Parse(s, CultureInfo.InvariantCulture);
            if (typeof(T) == typeof(short))
                return (T)(object)short.Parse(s, CultureInfo.InvariantCulture);

            throw new NotSupportedException($"Number type {typeof(T)} not supported for sets");
        }

        /// <summary>
        /// Formats a number for DynamoDB's string representation.
        /// Uses InvariantCulture for culture-safe formatting.
        /// </summary>
        private static string FormatNumber<T>(T value) where T : struct
        {
            return value switch
            {
                int i => i.ToString(CultureInfo.InvariantCulture),
                long l => l.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture),
                double d => d.ToString(CultureInfo.InvariantCulture),
                decimal m => m.ToString(CultureInfo.InvariantCulture),
                byte b => b.ToString(CultureInfo.InvariantCulture),
                short s => s.ToString(CultureInfo.InvariantCulture),
                _ => throw new NotSupportedException(
                    $"Number type {typeof(T)} not supported for sets"
                ),
            };
        }
    }

    private sealed class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        internal static readonly ByteArrayComparer Instance = new();

        public bool Equals(byte[]? x, byte[]? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            if (obj.Length == 0)
                return 0;

            var hash = 17;
            foreach (var b in obj)
                hash = (hash * 31) + b;

            return hash;
        }
    }
}
