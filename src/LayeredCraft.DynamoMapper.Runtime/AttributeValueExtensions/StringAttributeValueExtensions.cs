using DynamoMapper.Runtime;

// ReSharper disable MemberCanBePrivate.Global

namespace Amazon.DynamoDBv2.Model;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> to simplify extraction of typed values from DynamoDB attribute
///     dictionaries.
/// </summary>
public static class StringAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Tries to get a nullable string value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable string value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNullableString(
            string key,
            out string? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            if (attribute.IsNull)
                return true;

            value = attribute!.GetString(kind);
            return true;
        }

        /// <summary>Gets a nullable string value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The string value if the key exists and contains a non-NULL value; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public string? GetNullableString(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        ) => attributes.TryGetNullableString(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Tries to get a string value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The string value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetString(
            string key,
            out string? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            value = null;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            value = attribute!.GetString(kind);
            return true;
        }

        /// <summary>Gets a string value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The string value if the key exists and contains a non-NULL value; otherwise
        ///     <see cref="string.Empty" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public string GetString(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        ) =>
            attributes.TryGetString(key, out var value, requiredness, kind) ? value! : string.Empty;

        /// <summary>Sets a string value in the attribute dictionary, honoring omit rules.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">
        ///     The string value to set (can be null). When null and not omitted, a DynamoDB NULL is stored.
        /// </param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetString(
            string key,
            string? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            if (value.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value.ToAttributeValue(kind);

            return attributes;
        }
    }
}
