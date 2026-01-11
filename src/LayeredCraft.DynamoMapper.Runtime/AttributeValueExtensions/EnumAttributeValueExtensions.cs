using DynamoMapper.Runtime;

namespace Amazon.DynamoDBv2.Model;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> for enum values.
/// </summary>
public static class EnumAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Gets an enum value from the attribute dictionary.</summary>
        /// <typeparam name="TEnum">The enum type to parse.</typeparam>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the key doesn't exist or parsing fails.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The enum value if the key exists and is valid; otherwise <paramref name="defaultValue" />
        ///     if the key is missing (when Optional) or parsing fails.
        /// </returns>
        public TEnum GetEnum<TEnum>(
            string key,
            TEnum defaultValue,
            Requiredness requiredness = Requiredness.InferFromNullability
        )
            where TEnum : struct =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? (TEnum)Enum.Parse(typeof(TEnum), value!.S)
                : defaultValue;

        /// <summary>Gets an enum value from the attribute dictionary, expecting a specific format.</summary>
        /// <typeparam name="TEnum">The enum type to parse.</typeparam>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the key doesn't exist or parsing fails.</param>
        /// <param name="format">
        ///     The format that was used to serialize the enum. Valid formats: "G" or "g"
        ///     (general/name), "D" or "d" (decimal), "X" or "x" (hexadecimal), "F" or "f" (flags). This
        ///     parameter is for documentation purposes; parsing handles all formats.
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The enum value if the key exists and is valid; otherwise <paramref name="defaultValue" />
        ///     if the key is missing (when Optional) or parsing fails.
        /// </returns>
        public TEnum GetEnum<TEnum>(
            string key,
            TEnum defaultValue,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        )
            where TEnum : struct =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? (TEnum)Enum.Parse(typeof(TEnum), value!.S)
                : defaultValue;

        /// <summary>Gets a nullable enum value from the attribute dictionary.</summary>
        /// <typeparam name="TEnum">The enum type to parse.</typeparam>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The enum value if the key exists and is valid; otherwise <c>null</c> if the key is missing
        ///     or the attribute has a DynamoDB NULL value.
        /// </returns>
        public TEnum? GetNullableEnum<TEnum>(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        )
            where TEnum : struct =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static TEnum? (value) =>
                        value.IsNotNull ? (TEnum)Enum.Parse(typeof(TEnum), value!.S) : null
                );

        /// <summary>Gets a nullable enum value from the attribute dictionary, expecting a specific format.</summary>
        /// <typeparam name="TEnum">The enum type to parse.</typeparam>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The format that was used to serialize the enum. Valid formats: "G" or "g"
        ///     (general/name), "D" or "d" (decimal), "X" or "x" (hexadecimal), "F" or "f" (flags). This
        ///     parameter is for documentation purposes; parsing handles all formats.
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The enum value if the key exists and is valid; otherwise <c>null</c> if the key is missing
        ///     or the attribute has a DynamoDB NULL value.
        /// </returns>
        public TEnum? GetNullableEnum<TEnum>(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        )
            where TEnum : struct =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    TEnum? (value) =>
                        value.IsNotNull ? (TEnum)Enum.Parse(typeof(TEnum), value!.S) : null
                );

        /// <summary>Sets an enum value in the attribute dictionary.</summary>
        /// <typeparam name="TEnum">The enum type to set.</typeparam>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The enum value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetEnum<TEnum>(
            string key,
            TEnum? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
            where TEnum : struct, Enum
        {
            var stringValue = value?.ToString();
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        /// <summary>Sets an enum value in the attribute dictionary using a specific format.</summary>
        /// <typeparam name="TEnum">The enum type to set.</typeparam>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The enum value to set (can be null).</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string. Valid formats:
        ///     "G" or "g" (general/name), "D" or "d" (decimal), "X" or "x" (hexadecimal), "F" or "f" (flags).
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
        public Dictionary<string, AttributeValue> SetEnum<TEnum>(
            string key,
            TEnum? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
            where TEnum : struct, Enum
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
