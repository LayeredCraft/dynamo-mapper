using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

public static partial class AttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Gets a boolean value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The boolean value if the key exists and contains a non-NULL value; otherwise <c>false</c>
        ///     if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public bool GetBool(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value)
            && value.IsNotNull
            && (value?.BOOL ?? false);

        /// <summary>Gets a nullable boolean value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The boolean value if the key exists and contains a non-NULL value; otherwise <c>null</c>
        ///     if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public bool? GetNullableBool(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(static value => value.IsNull ? null : value.BOOL);

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
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetBool(
            string key,
            bool? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            if (ShouldSet(value, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { BOOL = value.Value };

            return attributes;
        }
    }
}
