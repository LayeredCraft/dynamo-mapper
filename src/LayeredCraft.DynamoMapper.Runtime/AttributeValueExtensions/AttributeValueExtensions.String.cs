using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> to simplify extraction of typed values from DynamoDB attribute
///     dictionaries.
/// </summary>
public static partial class AttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
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
        ) => attributes.GetNullableValue(key, requiredness).GetNullableString(kind);

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
        ) => attributes.GetValue(key, requiredness).GetString(kind);

        /// <summary>Sets a string value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The string value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetString(
            string key,
            string? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            if (ShouldSet(value, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = value };

            return attributes;
        }
    }
}
