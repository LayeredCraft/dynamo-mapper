using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> for <see cref="Guid" /> values.
/// </summary>
public static class GuidAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Gets a <see cref="Guid" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="Guid" /> value if the key exists and is valid; otherwise
        ///     <see cref="Guid.Empty" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public Guid GetGuid(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? Guid.Parse(value!.S)
                : Guid.Empty;

        /// <summary>Gets a nullable <see cref="Guid" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="Guid" /> value if the key exists and is valid; otherwise <c>null</c> if the
        ///     key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public Guid? GetNullableGuid(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(static Guid? (value) => value.IsNotNull ? Guid.Parse(value.S) : null);

        /// <summary>
        ///     Gets a <see cref="Guid" /> value from the attribute dictionary using an exact format
        ///     string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing. Valid formats: "N" (32 digits),
        ///     "D" (hyphens), "B" (braces), "P" (parentheses), "X" (hexadecimal).
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="Guid" /> value if the key exists and matches the format; otherwise
        ///     <see cref="Guid.Empty" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public Guid GetGuid(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? Guid.ParseExact(value!.S, format)
                : Guid.Empty;

        /// <summary>
        ///     Gets a nullable <see cref="Guid" /> value from the attribute dictionary using an exact
        ///     format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing. Valid formats: "N" (32 digits),
        ///     "D" (hyphens), "B" (braces), "P" (parentheses), "X" (hexadecimal).
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="Guid" /> value if the key exists and matches the format; otherwise
        ///     <c>null</c> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public Guid? GetNullableGuid(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(Guid? (value) => value.IsNotNull ? Guid.ParseExact(value.S, format) : null);

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
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetGuid(
            string key,
            Guid? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString();
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

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
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetGuid(
            string key,
            Guid? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(format);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }
    }
}
