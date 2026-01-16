using System.Globalization;
using DynamoMapper.Runtime;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable OutParameterValueIsAlwaysDiscarded.Global
// ReSharper disable UnusedParameter.Global

namespace Amazon.DynamoDBv2.Model;

/// <summary>
/// Extension methods for collection types (Lists, Maps, Sets).
/// </summary>
public static class CollectionAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        // ==================== LIST OPERATIONS ====================

        /// <summary>Tries to get a list of elements from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The list value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.L" />
        ///     .
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetList<T>(
            string key,
            out List<T> value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.L
        )
        {
            value = [];
            if (!attributes.TryGetValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue!.IsNull)
                return true;

            var list = attributeValue!.L ?? [];
            value = list.Select(Dictionary<string, AttributeValue>.ConvertFromAttributeValue<T>)
                .ToList();
            return true;
        }

        /// <summary>Gets a list of elements from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.L" />.</param>
        /// <returns>
        ///     The list value if the key exists; otherwise an empty list if the key is missing or the attribute has a
        ///     DynamoDB NULL value.
        /// </returns>
        public List<T> GetList<T>(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.L
        ) => attributes.TryGetList<T>(key, out var value, requiredness, kind) ? value : [];

        /// <summary>Tries to get a nullable list of elements from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable list value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.L" />
        ///     .
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNullableList<T>(
            string key,
            out List<T>? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.L
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue.IsNull)
                return true;

            var list = attributeValue!.L ?? [];
            value = list.Select(Dictionary<string, AttributeValue>.ConvertFromAttributeValue<T>)
                .ToList();
            return true;
        }

        /// <summary>Gets a nullable list from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.L" />.</param>
        /// <returns>
        ///     The list value if the key exists and contains a non-NULL value; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public List<T>? GetNullableList<T>(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.L
        ) =>
            attributes.TryGetNullableList<T>(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Sets a list in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetList<T>(
            string key,
            IEnumerable<T>? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.L
        )
        {
            if (value is null && omitNullStrings)
                return attributes;

            if (value is null)
            {
                attributes[key] = new AttributeValue { NULL = true };
                return attributes;
            }

            var list = value
                .Select(Dictionary<string, AttributeValue>.ConvertToAttributeValue)
                .ToList();

            // Empty lists ARE allowed in DynamoDB - respect omitEmptyStrings flag
            if (list.Count == 0 && omitEmptyStrings)
                return attributes;

            attributes[key] = new AttributeValue { L = list };
            return attributes;
        }

        // ==================== MAP OPERATIONS ====================

        /// <summary>Tries to get a map from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The map value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.M" />
        ///     .
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetMap<T>(
            string key,
            out Dictionary<string, T> value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.M
        )
        {
            value = new Dictionary<string, T>();
            if (!attributes.TryGetValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue!.IsNull)
                return true;

            var map = attributeValue!.M ?? new Dictionary<string, AttributeValue>();
            value = map.ToDictionary(
                kvp => kvp.Key,
                kvp => Dictionary<string, AttributeValue>.ConvertFromAttributeValue<T>(kvp.Value)
            );
            return true;
        }

        /// <summary>Gets a map from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.M" />.</param>
        /// <returns>
        ///     The map value if the key exists; otherwise an empty map if the key is missing or the attribute has a
        ///     DynamoDB NULL value.
        /// </returns>
        public Dictionary<string, T> GetMap<T>(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.M
        ) =>
            attributes.TryGetMap<T>(key, out var value, requiredness, kind)
                ? value
                : new Dictionary<string, T>();

        /// <summary>Tries to get a nullable map from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable map value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.M" />
        ///     .
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNullableMap<T>(
            string key,
            out Dictionary<string, T>? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.M
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue.IsNull)
                return true;

            var map = attributeValue!.M ?? new Dictionary<string, AttributeValue>();
            value = map.ToDictionary(
                kvp => kvp.Key,
                kvp => Dictionary<string, AttributeValue>.ConvertFromAttributeValue<T>(kvp.Value)
            );
            return true;
        }

        /// <summary>Gets a nullable map from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.M" />.</param>
        /// <returns>
        ///     The map value if the key exists and contains a non-NULL value; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public Dictionary<string, T>? GetNullableMap<T>(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.M
        ) => attributes.TryGetNullableMap<T>(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Sets a map in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetMap<T>(
            string key,
            IDictionary<string, T>? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.M
        )
        {
            if (value is null && omitNullStrings)
                return attributes;

            if (value is null)
            {
                attributes[key] = new AttributeValue { NULL = true };
                return attributes;
            }

            var map = value.ToDictionary(
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

        /// <summary>Tries to get a string set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The string set value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is
        ///     <see cref="DynamoKind.SS" />.
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetStringSet(
            string key,
            out HashSet<string> value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.SS
        )
        {
            value = [];
            if (!attributes.TryGetValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue!.IsNull)
                return true;

            var ss = attributeValue!.SS ?? [];
            value = [.. ss];
            return true;
        }

        /// <summary>Gets a string set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.SS" />.</param>
        /// <returns>
        ///     The string set value if the key exists; otherwise an empty set if the key is missing or the attribute
        ///     has a DynamoDB NULL value.
        /// </returns>
        public HashSet<string> GetStringSet(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.SS
        ) => attributes.TryGetStringSet(key, out var value, requiredness, kind) ? value : [];

        /// <summary>Tries to get a nullable string set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable string set value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is
        ///     <see cref="DynamoKind.SS" />.
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNullableStringSet(
            string key,
            out HashSet<string>? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.SS
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue.IsNull)
                return true;

            var ss = attributeValue!.SS ?? [];
            value = [.. ss];
            return true;
        }

        /// <summary>Gets a nullable string set.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.SS" />.</param>
        /// <returns>
        ///     The string set value if the key exists and contains a non-NULL value; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public HashSet<string>? GetNullableStringSet(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.SS
        ) =>
            attributes.TryGetNullableStringSet(key, out var value, requiredness, kind)
                ? value
                : null;

        /// <summary>Sets a string set in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetStringSet(
            string key,
            IEnumerable<string>? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.SS
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

        /// <summary>Tries to get a number set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The number set value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is
        ///     <see cref="DynamoKind.NS" />.
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNumberSet<T>(
            string key,
            out HashSet<T> value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.NS
        )
            where T : struct
        {
            value = [];
            if (!attributes.TryGetValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue!.IsNull)
                return true;

            var ns = attributeValue!.NS ?? [];
            value = [.. ns.Select(Dictionary<string, AttributeValue>.ParseNumber<T>)];
            return true;
        }

        /// <summary>Gets a number set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.NS" />.</param>
        /// <returns>
        ///     The number set value if the key exists; otherwise an empty set if the key is missing or the attribute
        ///     has a DynamoDB NULL value.
        /// </returns>
        public HashSet<T> GetNumberSet<T>(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.NS
        )
            where T : struct =>
            attributes.TryGetNumberSet<T>(key, out var value, requiredness, kind) ? value : [];

        /// <summary>Tries to get a nullable number set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable number set value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is
        ///     <see cref="DynamoKind.NS" />.
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNullableNumberSet<T>(
            string key,
            out HashSet<T>? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.NS
        )
            where T : struct
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue.IsNull)
                return true;

            var ns = attributeValue!.NS ?? [];
            value = [.. ns.Select(Dictionary<string, AttributeValue>.ParseNumber<T>)];
            return true;
        }

        /// <summary>Gets a nullable number set.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.NS" />.</param>
        /// <returns>
        ///     The number set value if the key exists and contains a non-NULL value; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public HashSet<T>? GetNullableNumberSet<T>(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.NS
        )
            where T : struct =>
            attributes.TryGetNullableNumberSet<T>(key, out var value, requiredness, kind)
                ? value
                : null;

        /// <summary>Sets a number set in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetNumberSet<T>(
            string key,
            IEnumerable<T>? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.NS
        )
            where T : struct
        {
            if (value is null && omitNullStrings)
                return attributes;

            var set =
                value?.Distinct().Select(Dictionary<string, AttributeValue>.FormatNumber).ToList()
                ?? [];

            // DynamoDB does NOT allow empty sets - always omit
            if (set.Count == 0)
                return attributes;

            attributes[key] = new AttributeValue { NS = set };
            return attributes;
        }

        // ==================== BINARY SET OPERATIONS ====================

        /// <summary>Tries to get a binary set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The binary set value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is
        ///     <see cref="DynamoKind.BS" />.
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetBinarySet(
            string key,
            out HashSet<byte[]> value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BS
        )
        {
            value = new HashSet<byte[]>(ByteArrayComparer.Instance);
            if (!attributes.TryGetValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue!.IsNull)
                return true;

            var bs = attributeValue!.BS ?? [];
            value = new HashSet<byte[]>(
                bs.Select(stream => stream.ToArray()),
                ByteArrayComparer.Instance
            );
            return true;
        }

        /// <summary>Gets a binary set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.BS" />.</param>
        /// <returns>
        ///     The binary set value if the key exists; otherwise an empty set if the key is missing or the attribute
        ///     has a DynamoDB NULL value.
        /// </returns>
        public HashSet<byte[]> GetBinarySet(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BS
        ) =>
            attributes.TryGetBinarySet(key, out var value, requiredness, kind)
                ? value
                : new HashSet<byte[]>(ByteArrayComparer.Instance);

        /// <summary>Tries to get a nullable binary set from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable binary set value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">
        ///     The DynamoDB attribute kind to interpret. Default is
        ///     <see cref="DynamoKind.BS" />.
        /// </param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNullableBinarySet(
            string key,
            out HashSet<byte[]>? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BS
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attributeValue))
                return false;

            if (attributeValue.IsNull)
                return true;

            var bs = attributeValue!.BS ?? [];
            value = new HashSet<byte[]>(
                bs.Select(stream => stream.ToArray()),
                ByteArrayComparer.Instance
            );
            return true;
        }

        /// <summary>Gets a nullable binary set.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret. Default is <see cref="DynamoKind.BS" />.</param>
        /// <returns>
        ///     The binary set value if the key exists and contains a non-NULL value; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public HashSet<byte[]>? GetNullableBinarySet(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BS
        ) =>
            attributes.TryGetNullableBinarySet(key, out var value, requiredness, kind)
                ? value
                : null;

        /// <summary>Sets a binary set in the attribute dictionary.</summary>
        public Dictionary<string, AttributeValue> SetBinarySet(
            string key,
            IEnumerable<byte[]>? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.BS
        )
        {
            if (value is null && omitNullStrings)
                return attributes;

            var set =
                value
                    ?.Where(bytes => bytes is not null)
                    .Distinct(ByteArrayComparer.Instance)
                    .Select(bytes => new MemoryStream(bytes!))
                    .ToList()
                ?? new List<MemoryStream>();

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
                return (T)
                    (object)int.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(long))
                return (T)
                    (object)long.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(float))
                return (T)
                    (object)float.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(double))
                return (T)
                    (object)double.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(decimal))
                return (T)
                    (object)decimal.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(byte))
                return (T)
                    (object)byte.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);
            if (underlyingType == typeof(short))
                return (T)
                    (object)short.Parse(av.GetString(DynamoKind.N), CultureInfo.InvariantCulture);

            // DateTime
            if (underlyingType == typeof(DateTime))
                return (T)
                    (object)
                        DateTime.Parse(av.GetString(DynamoKind.S), CultureInfo.InvariantCulture);

            // DateTimeOffset
            if (underlyingType == typeof(DateTimeOffset))
                return (T)
                    (object)
                        DateTimeOffset.Parse(
                            av.GetString(DynamoKind.S),
                            CultureInfo.InvariantCulture
                        );

            // TimeSpan
            if (underlyingType == typeof(TimeSpan))
                return (T)
                    (object)
                        TimeSpan.Parse(av.GetString(DynamoKind.S), CultureInfo.InvariantCulture);

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
                return ((Guid)(object)value).ToString().ToAttributeValue(DynamoKind.S);

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
        private static T ParseNumber<T>(string s)
            where T : struct
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
        private static string FormatNumber<T>(T value)
            where T : struct
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
