using DynamoMapper.Runtime;

namespace Amazon.DynamoDBv2.Model;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> for boolean values.
/// </summary>
public static class BooleanAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Gets a boolean value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a boolean.</param>
        /// <returns>
        ///     The boolean value if the key exists and contains a non-NULL value; otherwise <c>false</c>
        ///     if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public bool GetBool(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BOOL
        ) => attributes.GetValue(key, requiredness).GetBool(kind);

        /// <summary>Gets a nullable boolean value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a boolean.</param>
        /// <returns>
        ///     The boolean value if the key exists and contains a non-NULL value; otherwise <c>null</c>
        ///     if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public bool? GetNullableBool(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BOOL
        ) => attributes.GetNullableValue(key, requiredness).GetNullableBool(kind);

        /// <summary>Sets a boolean value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The boolean value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.BOOL" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetBool(
            string key,
            bool? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            if (value.ShouldSet(omitNullStrings))
                attributes[key] = value.ToAttributeValue(kind);

            return attributes;
        }
    }
}
