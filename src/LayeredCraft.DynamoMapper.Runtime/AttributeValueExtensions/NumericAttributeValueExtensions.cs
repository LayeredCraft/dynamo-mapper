using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> for numeric values.
/// </summary>
public static class NumericAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Gets an integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The integer value if the key exists and is valid; otherwise <c>0</c> if the key is missing
        ///     or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public int GetInt(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? int.Parse(value!.N, NumberStyles.Integer, CultureInfo.InvariantCulture)
                : 0;

        /// <summary>Gets a nullable integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The integer value if the key exists and is valid; otherwise <c>null</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public int? GetNullableInt(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static int? (value) =>
                        value.IsNotNull
                            ? int.Parse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture)
                            : null
                );

        /// <summary>Gets a long integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The long value if the key exists and is valid; otherwise <c>0</c> if the key is missing or
        ///     the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public long GetLong(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? long.Parse(value!.N, NumberStyles.Integer, CultureInfo.InvariantCulture)
                : 0;

        /// <summary>Gets a nullable long integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The long value if the key exists and is valid; otherwise <c>null</c> if the key is missing
        ///     or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public long? GetNullableLong(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static long? (value) =>
                        value.IsNotNull
                            ? long.Parse(
                                value.N,
                                NumberStyles.Integer,
                                CultureInfo.InvariantCulture
                            )
                            : null
                );

        /// <summary>Sets an integer value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The integer value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetInt(
            string key,
            int? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { N = stringValue };

            return attributes;
        }

        /// <summary>Sets a long integer value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The long value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetLong(
            string key,
            long? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { N = stringValue };

            return attributes;
        }

        /// <summary>Gets a single-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The float value if the key exists and is valid; otherwise <c>0f</c> if the key is missing
        ///     or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public float GetFloat(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? float.Parse(value!.N, NumberStyles.Float, CultureInfo.InvariantCulture)
                : 0f;

        /// <summary>Gets a nullable single-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The float value if the key exists and is valid; otherwise <c>null</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public float? GetNullableFloat(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static float? (value) =>
                        value.IsNotNull
                            ? float.Parse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture)
                            : null
                );

        /// <summary>Gets a double-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The double value if the key exists and is valid; otherwise <c>0.0</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public double GetDouble(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? double.Parse(value!.N, NumberStyles.Float, CultureInfo.InvariantCulture)
                : 0.0;

        /// <summary>Gets a nullable double-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The double value if the key exists and is valid; otherwise <c>null</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public double? GetNullableDouble(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static double? (value) =>
                        value.IsNotNull
                            ? double.Parse(
                                value.N,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture
                            )
                            : null
                );

        /// <summary>Gets a decimal value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The decimal value if the key exists and is valid; otherwise <c>0m</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public decimal GetDecimal(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? decimal.Parse(value!.N, NumberStyles.Float, CultureInfo.InvariantCulture)
                : 0m;

        /// <summary>Gets a nullable decimal value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The decimal value if the key exists and is valid; otherwise <c>null</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public decimal? GetNullableDecimal(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static decimal? (value) =>
                        value.IsNotNull
                            ? decimal.Parse(
                                value.N,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture
                            )
                            : null
                );

        /// <summary>Sets a single-precision floating-point value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The float value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetFloat(
            string key,
            float? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { N = stringValue };

            return attributes;
        }

        /// <summary>Sets a double-precision floating-point value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The double value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDouble(
            string key,
            double? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { N = stringValue };

            return attributes;
        }

        /// <summary>Sets a decimal value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The decimal value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDecimal(
            string key,
            decimal? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { N = stringValue };

            return attributes;
        }
    }
}
