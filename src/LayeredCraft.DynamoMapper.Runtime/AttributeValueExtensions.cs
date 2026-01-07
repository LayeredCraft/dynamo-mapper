using System.Globalization;
using Amazon.DynamoDBv2.Model;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace DynamoMapper.Runtime;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> to simplify extraction of typed values from DynamoDB attribute
///     dictionaries.
/// </summary>
public static class AttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        #region String Methods

        /// <summary>Gets a nullable string value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The string value if the key exists and contains a non-NULL value; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public string? GetNullableString(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(static value => value.IsNull ? null : value.S);

        /// <summary>Gets a string value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The string value if the key exists and contains a non-NULL value; otherwise
        ///     <see cref="string.Empty" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        public string GetString(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? value!.S
                : string.Empty;

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

        #endregion

        #region Boolean Methods

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

        #endregion

        #region Integer Methods

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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { N = stringValue };

            return attributes;
        }

        #endregion

        #region Floating Point Methods

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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { N = stringValue };

            return attributes;
        }

        #endregion

        #region Guid Methods

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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        #endregion

        #region DateTime Methods

        /// <summary>Gets a <see cref="DateTime" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="DateTime" /> value if the key exists and is valid; otherwise
        ///     <see cref="DateTime.MinValue" /> if the key is missing or the attribute has a DynamoDB NULL
        ///     value.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" /> for ISO-8601 format.
        /// </remarks>
        public DateTime GetDateTime(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? DateTime.Parse(
                    value!.S,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                )
                : DateTime.MinValue;

        /// <summary>
        ///     Gets a <see cref="DateTime" /> value from the attribute dictionary using an exact format
        ///     string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "yyyy-MM-dd", "yyyyMMdd",
        ///     "MM/dd/yyyy").
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="DateTime" /> value if the key exists and matches the format; otherwise
        ///     <see cref="DateTime.MinValue" /> if the key is missing or the attribute has a DynamoDB NULL
        ///     value.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" />.
        /// </remarks>
        public DateTime GetDateTime(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? DateTime.ParseExact(
                    value!.S,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                )
                : DateTime.MinValue;

        /// <summary>Gets a nullable <see cref="DateTime" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="DateTime" /> value if the key exists and is valid; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" /> for ISO-8601 format.
        /// </remarks>
        public DateTime? GetNullableDateTime(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static DateTime? (value) =>
                        value.IsNotNull
                            ? DateTime.Parse(
                                value.S,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind
                            )
                            : null
                );

        /// <summary>
        ///     Gets a nullable <see cref="DateTime" /> value from the attribute dictionary using an exact
        ///     format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "yyyy-MM-dd", "yyyyMMdd",
        ///     "MM/dd/yyyy").
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="DateTime" /> value if the key exists and matches the format; otherwise
        ///     <c>null</c> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" />.
        /// </remarks>
        public DateTime? GetNullableDateTime(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    DateTime? (value) =>
                        value.IsNotNull
                            ? DateTime.ParseExact(
                                value.S,
                                format,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind
                            )
                            : null
                );

        /// <summary>Gets a <see cref="DateTimeOffset" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="DateTimeOffset" /> value if the key exists and is valid; otherwise
        ///     <see cref="DateTimeOffset.MinValue" /> if the key is missing or the attribute has a DynamoDB
        ///     NULL value.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" /> for ISO-8601 format.
        /// </remarks>
        public DateTimeOffset GetDateTimeOffset(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? DateTimeOffset.Parse(
                    value!.S,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                )
                : DateTimeOffset.MinValue;

        /// <summary>Gets a nullable <see cref="DateTimeOffset" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="DateTimeOffset" /> value if the key exists and is valid; otherwise
        ///     <c>null</c> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" /> for ISO-8601 format.
        /// </remarks>
        public DateTimeOffset? GetNullableDateTimeOffset(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static DateTimeOffset? (value) =>
                        value.IsNotNull
                            ? DateTimeOffset.Parse(
                                value.S,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind
                            )
                            : null
                );

        /// <summary>
        ///     Gets a <see cref="DateTimeOffset" /> value from the attribute dictionary using an exact
        ///     format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "yyyy-MM-dd", "yyyyMMdd",
        ///     "o").
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="DateTimeOffset" /> value if the key exists and matches the format;
        ///     otherwise <see cref="DateTimeOffset.MinValue" /> if the key is missing or the attribute has a
        ///     DynamoDB NULL value.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" />.
        /// </remarks>
        public DateTimeOffset GetDateTimeOffset(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? DateTimeOffset.ParseExact(
                    value!.S,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                )
                : DateTimeOffset.MinValue;

        /// <summary>
        ///     Gets a nullable <see cref="DateTimeOffset" /> value from the attribute dictionary using an
        ///     exact format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "yyyy-MM-dd", "yyyyMMdd",
        ///     "o").
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="DateTimeOffset" /> value if the key exists and matches the format;
        ///     otherwise <c>null</c> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" />.
        /// </remarks>
        public DateTimeOffset? GetNullableDateTimeOffset(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    DateTimeOffset? (value) =>
                        value.IsNotNull
                            ? DateTimeOffset.ParseExact(
                                value.S,
                                format,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind
                            )
                            : null
                );

        /// <summary>Gets a <see cref="TimeSpan" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and is valid; otherwise
        ///     <see cref="TimeSpan.Zero" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan GetTimeSpan(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? TimeSpan.Parse(value!.S, CultureInfo.InvariantCulture)
                : TimeSpan.Zero;

        /// <summary>Gets a nullable <see cref="TimeSpan" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and is valid; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan? GetNullableTimeSpan(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    static TimeSpan? (value) =>
                        value.IsNotNull
                            ? TimeSpan.Parse(value.S, CultureInfo.InvariantCulture)
                            : null
                );

        /// <summary>
        ///     Gets a <see cref="TimeSpan" /> value from the attribute dictionary using an exact format
        ///     string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "c", "g", "G", or custom
        ///     patterns like "hh\\:mm\\:ss").
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and matches the format; otherwise
        ///     <see cref="TimeSpan.Zero" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan GetTimeSpan(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes.TryGetValue(key, requiredness, out var value) && value.IsNotNull
                ? TimeSpan.ParseExact(value!.S, format, CultureInfo.InvariantCulture)
                : TimeSpan.Zero;

        /// <summary>
        ///     Gets a nullable <see cref="TimeSpan" /> value from the attribute dictionary using an exact
        ///     format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "c", "g", "G", or custom
        ///     patterns like "hh\\:mm\\:ss").
        /// </param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and matches the format; otherwise
        ///     <c>null</c> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan? GetNullableTimeSpan(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability
        ) =>
            attributes
                .GetNullableValue(key, requiredness)
                .Map(
                    TimeSpan? (value) =>
                        value.IsNotNull
                            ? TimeSpan.ParseExact(value.S, format, CultureInfo.InvariantCulture)
                            : null
                );

        /// <summary>Sets a <see cref="DateTime" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateTime" /> value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDateTime(
            string key,
            DateTime? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString("o", CultureInfo.InvariantCulture);
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        /// <summary>Sets a <see cref="DateTimeOffset" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateTimeOffset" /> value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDateTimeOffset(
            string key,
            DateTimeOffset? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString("o", CultureInfo.InvariantCulture);
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        /// <summary>Sets a <see cref="TimeSpan" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="TimeSpan" /> value to set (can be null).</param>
        /// <param name="omitEmptyStrings">
        ///     Whether to omit empty string values from the DynamoDB item. Default
        ///     is <c>false</c>.
        /// </param>
        /// <param name="omitNullStrings">
        ///     Whether to omit null string values from the DynamoDB item. Default is
        ///     <c>true</c>.
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetTimeSpan(
            string key,
            TimeSpan? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString("c", CultureInfo.InvariantCulture);
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        /// <summary>Sets a <see cref="DateTime" /> value in the attribute dictionary using a specific format.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateTime" /> value to set (can be null).</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "yyyyMMdd", "o").
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
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetDateTime(
            string key,
            DateTime? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(format, CultureInfo.InvariantCulture);
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        /// <summary>
        ///     Sets a <see cref="DateTimeOffset" /> value in the attribute dictionary using a specific
        ///     format.
        /// </summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateTimeOffset" /> value to set (can be null).</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "o").
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
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetDateTimeOffset(
            string key,
            DateTimeOffset? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(format, CultureInfo.InvariantCulture);
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        /// <summary>Sets a <see cref="TimeSpan" /> value in the attribute dictionary using a specific format.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="TimeSpan" /> value to set (can be null).</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g., "c",
        ///     "g", "G").
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
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetTimeSpan(
            string key,
            TimeSpan? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true
        )
        {
            var stringValue = value?.ToString(format, CultureInfo.InvariantCulture);
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        #endregion

        #region Enum Methods

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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
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
            if (ShouldSet(stringValue, omitEmptyStrings, omitNullStrings))
                attributes[key] = value is null
                    ? new AttributeValue { NULL = true }
                    : new AttributeValue { S = stringValue };

            return attributes;
        }

        #endregion

        #region Utilities

        /// <summary>
        ///     Retrieves an attribute value from the dictionary, returning a NULL AttributeValue if the
        ///     key is missing and the attribute is optional.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <returns>
        ///     The <see cref="AttributeValue" /> if the key exists; otherwise a NULL
        ///     <see cref="AttributeValue" /> if <paramref name="requiredness" /> is
        ///     <see cref="Requiredness.Optional" /> or <see cref="Requiredness.InferFromNullability" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the key is missing and
        ///     <paramref name="requiredness" /> is <see cref="Requiredness.Required" />.
        /// </exception>
        /// <remarks>
        ///     This helper is used by <c>GetNullable*</c> methods to handle missing attributes. When the
        ///     key is missing and the attribute is optional, it returns an AttributeValue with NULL = true,
        ///     allowing the caller to distinguish between a missing attribute and an explicit DynamoDB NULL
        ///     value.
        /// </remarks>
        private AttributeValue GetNullableValue(string key, Requiredness requiredness)
        {
            if (!attributes.TryGetValue(key, out var attributeValue))
                return requiredness switch
                {
                    Requiredness.Required => throw new InvalidOperationException(
                        $"The DynamoDB item does not contain an attribute named '{key}'."
                    ),
                    Requiredness.Optional or Requiredness.InferFromNullability => new AttributeValue
                    {
                        NULL = true,
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(requiredness)),
                };

            return attributeValue;
        }

        /// <summary>Attempts to retrieve an attribute value from the dictionary with requiredness validation.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="value">
        ///     When this method returns, contains the <see cref="AttributeValue" /> if the key
        ///     exists; otherwise <c>null</c> if the key is missing and <paramref name="requiredness" /> is
        ///     <see cref="Requiredness.Optional" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the key exists in the dictionary; otherwise <c>false</c> if the key is
        ///     missing and <paramref name="requiredness" /> is <see cref="Requiredness.Optional" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the key is missing and
        ///     <paramref name="requiredness" /> is <see cref="Requiredness.Required" /> or
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </exception>
        /// <remarks>
        ///     This helper is used by non-nullable <c>Get*</c> methods to handle missing attributes.
        ///     Unlike <see cref="GetNullableValue" />, this method returns <c>false</c> with a <c>null</c> out
        ///     parameter when the key is missing and the attribute is optional, allowing callers to provide
        ///     default values for non-nullable types.
        /// </remarks>
        private bool TryGetValue(string key, Requiredness requiredness, out AttributeValue? value)
        {
            value = null;

            if (!attributes.TryGetValue(key, out var attributeValue))
            {
                value = requiredness switch
                {
                    Requiredness.Required or Requiredness.InferFromNullability =>
                        throw new InvalidOperationException(
                            $"The DynamoDB item does not contain an attribute named '{key}'."
                        ),
                    Requiredness.Optional => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(requiredness)),
                };

                return false;
            }

            value = attributeValue;
            return true;
        }

        #endregion
    }

    extension(AttributeValue? attributeValue)
    {
        /// <summary>
        ///     Gets a value indicating whether this <see cref="AttributeValue" /> represents a DynamoDB
        ///     NULL value.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the attribute value is <c>null</c> or has its
        ///     <see cref="AttributeValue.NULL" /> property set to <c>true</c>; otherwise <c>false</c>.
        /// </value>
        /// <remarks>
        ///     This extension property provides a convenient way to check if an AttributeValue represents
        ///     a DynamoDB NULL. It handles both the case where the AttributeValue itself is null and where it
        ///     explicitly has NULL = true.
        /// </remarks>
        public bool IsNull => attributeValue?.NULL is true;

        public bool IsNotNull => attributeValue?.NULL is null or false;
    }

    private static bool ShouldSet(string? value, bool omitEmptyStrings, bool omitNullStrings) =>
        value switch
        {
            null when omitNullStrings => false,
            { Length: 0 } when omitEmptyStrings => false,
            _ => true,
        };

    private static bool ShouldSet(bool? value, bool omitNullStrings) =>
        value switch
        {
            null when omitNullStrings => false,
            _ => true,
        };
}
