using System.Globalization;
using DynamoMapper.Runtime;

namespace Amazon.DynamoDBv2.Model;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> for numeric values.
/// </summary>
public static class NumericAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Tries to get an integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The integer value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetInt(
            string key,
            out int value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = 0;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetString(kind);
            value =
                stringValue.Length == 0
                    ? 0
                    : int.Parse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets an integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The integer value if the key exists and is valid; otherwise <c>0</c> if the key is missing
        ///     or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public int GetInt(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetInt(key, out var value, requiredness, kind) ? value : 0;

        /// <summary>Tries to get a nullable integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable integer value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetNullableInt(
            string key,
            out int? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetNullableString(kind);
            value = stringValue is null
                ? null
                : int.Parse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a nullable integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The integer value if the key exists and is valid; otherwise <c>null</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public int? GetNullableInt(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetNullableInt(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Tries to get a long integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The long value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetLong(
            string key,
            out long value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = 0;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetString(kind);
            value =
                stringValue.Length == 0
                    ? 0
                    : long.Parse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a long integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The long value if the key exists and is valid; otherwise <c>0</c> if the key is missing or
        ///     the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public long GetLong(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetLong(key, out var value, requiredness, kind) ? value : 0;

        /// <summary>Tries to get a nullable long integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable long value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetNullableLong(
            string key,
            out long? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetNullableString(kind);
            value = stringValue is null
                ? null
                : long.Parse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a nullable long integer value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The long value if the key exists and is valid; otherwise <c>null</c> if the key is missing
        ///     or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public long? GetNullableLong(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetNullableLong(key, out var value, requiredness, kind) ? value : null;

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.N" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetInt(
            string key,
            int? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.N
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.N" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetLong(
            string key,
            long? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.N
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

            return attributes;
        }

        /// <summary>Tries to get a single-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The float value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetFloat(
            string key,
            out float value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = 0f;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetString(kind);
            value =
                stringValue.Length == 0
                    ? 0f
                    : float.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a single-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The float value if the key exists and is valid; otherwise <c>0f</c> if the key is missing
        ///     or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public float GetFloat(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetFloat(key, out var value, requiredness, kind) ? value : 0f;

        /// <summary>
        ///     Tries to get a nullable single-precision floating-point value from the attribute
        ///     dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable float value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetNullableFloat(
            string key,
            out float? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetNullableString(kind);
            value = stringValue is null
                ? null
                : float.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a nullable single-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The float value if the key exists and is valid; otherwise <c>null</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public float? GetNullableFloat(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetNullableFloat(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Tries to get a double-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The double value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetDouble(
            string key,
            out double value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = 0.0;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetString(kind);
            value =
                stringValue.Length == 0
                    ? 0.0
                    : double.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a double-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The double value if the key exists and is valid; otherwise <c>0.0</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public double GetDouble(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetDouble(key, out var value, requiredness, kind) ? value : 0.0;

        /// <summary>
        ///     Tries to get a nullable double-precision floating-point value from the attribute
        ///     dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable double value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetNullableDouble(
            string key,
            out double? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetNullableString(kind);
            value = stringValue is null
                ? null
                : double.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a nullable double-precision floating-point value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The double value if the key exists and is valid; otherwise <c>null</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public double? GetNullableDouble(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetNullableDouble(key, out var value, requiredness, kind) ? value : null;

        /// <summary>Tries to get a decimal value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The decimal value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetDecimal(
            string key,
            out decimal value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = 0m;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetString(kind);
            value =
                stringValue.Length == 0
                    ? 0m
                    : decimal.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a decimal value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The decimal value if the key exists and is valid; otherwise <c>0m</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public decimal GetDecimal(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) => attributes.TryGetDecimal(key, out var value, requiredness, kind) ? value : 0m;

        /// <summary>Tries to get a nullable decimal value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable decimal value when found.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetNullableDecimal(
            string key,
            out decimal? value,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetNullableString(kind);
            value = stringValue is null
                ? null
                : decimal.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a nullable decimal value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a number.</param>
        /// <returns>
        ///     The decimal value if the key exists and is valid; otherwise <c>null</c> if the key is
        ///     missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public decimal? GetNullableDecimal(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.N
        ) =>
            attributes.TryGetNullableDecimal(key, out var value, requiredness, kind) ? value : null;

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.N" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetFloat(
            string key,
            float? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.N
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.N" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDouble(
            string key,
            double? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.N
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.N" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDecimal(
            string key,
            decimal? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.N
        )
        {
            var stringValue = value?.ToString(CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

            return attributes;
        }
    }
}
