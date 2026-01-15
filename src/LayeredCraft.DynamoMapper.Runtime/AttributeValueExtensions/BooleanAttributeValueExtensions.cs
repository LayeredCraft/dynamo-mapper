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
        /// <summary>Tries to get a nullable boolean value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable boolean value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a boolean.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetNullableBool(
            string key,
            out bool? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BOOL
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            value = attribute!.GetNullableBool(kind);
            return true;
        }

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
        ) => attributes.TryGetNullableBool(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Tries to get a boolean value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The boolean value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a boolean.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetBool(
            string key,
            out bool value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.BOOL
        )
        {
            value = false;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            value = attribute!.GetBool(kind);
            return true;
        }

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
        ) => attributes.TryGetBool(key, out var value, requiredness, kind) && value;

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
            DynamoKind kind = DynamoKind.BOOL
        )
        {
            if (value.ShouldSet(omitNullStrings))
                attributes[key] = value.ToAttributeValue(kind);

            return attributes;
        }
    }
}
