using DynamoMapper.Runtime;

namespace Amazon.DynamoDBv2.Model;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> for <see cref="Guid" /> values.
/// </summary>
public static class GuidAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>
        ///     Tries to get a <see cref="Guid" /> value from the attribute dictionary using an exact
        ///     format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The <see cref="Guid" /> value when found.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing. Valid formats: "N" (32 digits),
        ///     "D" (hyphens), "B" (braces), "P" (parentheses), "X" (hexadecimal). Default is "D".
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetGuid(
            string key,
            out Guid value,
            string format = "D",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            value = Guid.Empty;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetString(kind);
            value = stringValue.Length == 0 ? Guid.Empty : Guid.ParseExact(stringValue, format);
            return true;
        }

        /// <summary>
        ///     Gets a <see cref="Guid" /> value from the attribute dictionary using an exact format
        ///     string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing. Valid formats: "N" (32 digits),
        ///     "D" (hyphens), "B" (braces), "P" (parentheses), "X" (hexadecimal). Default is "D".
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="Guid" /> value if the key exists and matches the format; otherwise
        ///     <see cref="Guid.Empty" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public Guid GetGuid(
            string key,
            string format = "D",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        ) =>
            attributes.TryGetGuid(key, out var value, format, requiredness, kind)
                ? value
                : Guid.Empty;

        /// <summary>
        ///     Tries to get a nullable <see cref="Guid" /> value from the attribute dictionary using an
        ///     exact format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable <see cref="Guid" /> value when found.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing. Valid formats: "N" (32 digits),
        ///     "D" (hyphens), "B" (braces), "P" (parentheses), "X" (hexadecimal). Default is "D".
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNullableGuid(
            string key,
            out Guid? value,
            string format = "D",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            if (attribute.IsNull)
                return true;

            var stringValue = attribute!.GetNullableString(kind);
            value = stringValue is null ? null : Guid.ParseExact(stringValue, format);
            return true;
        }

        /// <summary>
        ///     Gets a nullable <see cref="Guid" /> value from the attribute dictionary using an exact
        ///     format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing. Valid formats: "N" (32 digits),
        ///     "D" (hyphens), "B" (braces), "P" (parentheses), "X" (hexadecimal). Default is "D".
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="Guid" /> value if the key exists and matches the format; otherwise
        ///     <c>null</c> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public Guid? GetNullableGuid(
            string key,
            string format = "D",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        ) =>
            attributes.TryGetNullableGuid(key, out var value, format, requiredness, kind)
                ? value
                : null;

        /// <summary>Sets a <see cref="Guid" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="Guid" /> value to set (can be null).</param>
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
        public Dictionary<string, AttributeValue> SetGuid(
            string key,
            Guid? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString();
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

            return attributes;
        }

        /// <summary>Sets a <see cref="Guid" /> value in the attribute dictionary using a specific format.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="Guid" /> value to set (can be null).</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string. Valid formats:
        ///     "N", "D", "B", "P", "X".
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
        public Dictionary<string, AttributeValue> SetGuid(
            string key,
            Guid? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString(format);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

            return attributes;
        }
    }
}
