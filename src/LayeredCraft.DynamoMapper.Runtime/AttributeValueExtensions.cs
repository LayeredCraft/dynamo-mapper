using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace DynamoMapper.Runtime;

/// <summary>
/// Extension methods for <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> and <see cref="AttributeValue"/>
/// to simplify extraction of typed values from DynamoDB attribute dictionaries.
/// </summary>
public static class AttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        #region String Methods

        /// <summary>
        /// Gets a nullable string value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The string value if the key exists, otherwise <c>null</c>.</returns>
        public string? GetNullableString(string key)
        {
            return attributes.TryGetValue(key, out var value) ? value.S : null;
        }

        /// <summary>
        /// Gets a string value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The string value if the key exists, otherwise <see cref="string.Empty"/>.</returns>
        public string GetString(string key) =>
            attributes.TryGetValue(key, out var value) ? value.S ?? string.Empty : string.Empty;

        /// <summary>Sets a string value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The string value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetString(string key, string value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable string value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable string value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableString(string key, string? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        #endregion

        #region Boolean Methods

        /// <summary>
        /// Gets a boolean value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The boolean value if the key exists, otherwise <c>false</c>.</returns>
        public bool GetBool(string key)
        {
            return attributes.TryGetValue(key, out var value) && (value.BOOL ?? false);
        }

        /// <summary>
        /// Gets a nullable boolean value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The boolean value if the key exists, otherwise <c>null</c>.</returns>
        public bool? GetNullableBool(string key)
        {
            return attributes.TryGetValue(key, out var value) ? value.BOOL : null;
        }

        /// <summary>Sets a boolean value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The boolean value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetBool(string key, bool value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable boolean value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable boolean value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableBool(string key, bool? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        #endregion

        #region Integer Methods

        /// <summary>
        /// Gets an integer value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The integer value if the key exists and is valid, otherwise <c>0</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public int GetInt(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? int.TryParse(
                    value.N,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : 0
                : 0;
        }

        /// <summary>
        /// Gets a nullable integer value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The integer value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public int? GetNullableInt(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? int.TryParse(
                    value.N,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : null
                : null;
        }

        /// <summary>
        /// Gets a long integer value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The long value if the key exists and is valid, otherwise <c>0</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public long GetLong(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? long.TryParse(
                    value.N,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : 0
                : 0;
        }

        /// <summary>
        /// Gets a nullable long integer value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The long value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public long? GetNullableLong(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? long.TryParse(
                    value.N,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : null
                : null;
        }

        /// <summary>Sets an integer value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The integer value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetInt(string key, int value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable integer value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable integer value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableInt(string key, int? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        /// <summary>Sets a long integer value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The long value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetLong(string key, long value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable long integer value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable long value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableLong(string key, long? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        #endregion

        #region Floating Point Methods

        /// <summary>
        /// Gets a single-precision floating-point value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The float value if the key exists and is valid, otherwise <c>0f</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public float GetFloat(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? float.TryParse(
                    value.N,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : 0f
                : 0f;
        }

        /// <summary>
        /// Gets a nullable single-precision floating-point value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The float value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public float? GetNullableFloat(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? float.TryParse(
                    value.N,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : null
                : null;
        }

        /// <summary>
        /// Gets a double-precision floating-point value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The double value if the key exists and is valid, otherwise <c>0.0</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public double GetDouble(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? double.TryParse(
                    value.N,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : 0.0
                : 0.0;
        }

        /// <summary>
        /// Gets a nullable double-precision floating-point value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The double value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public double? GetNullableDouble(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? double.TryParse(
                    value.N,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : null
                : null;
        }

        /// <summary>
        /// Gets a decimal value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The decimal value if the key exists and is valid, otherwise <c>0m</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public decimal GetDecimal(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? decimal.TryParse(
                    value.N,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : 0m
                : 0m;
        }

        /// <summary>
        /// Gets a nullable decimal value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The decimal value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public decimal? GetNullableDecimal(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? decimal.TryParse(
                    value.N,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var result
                )
                    ? result
                    : null
                : null;
        }

        /// <summary>Sets a single-precision floating-point value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The float value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetFloat(string key, float value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable single-precision floating-point value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable float value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableFloat(string key, float? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        /// <summary>Sets a double-precision floating-point value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The double value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDouble(string key, double value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable double-precision floating-point value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable double value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableDouble(string key, double? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        /// <summary>Sets a decimal value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The decimal value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDecimal(string key, decimal value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable decimal value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable decimal value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableDecimal(string key, decimal? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        #endregion

        #region Guid Methods

        /// <summary>
        /// Gets a <see cref="Guid"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="Guid"/> value if the key exists and is valid, otherwise <see cref="Guid.Empty"/>.</returns>
        public Guid GetGuid(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? Guid.TryParse(value.S, out var id)
                    ? id
                    : Guid.Empty
                : Guid.Empty;
        }

        /// <summary>
        /// Gets a nullable <see cref="Guid"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="Guid"/> value if the key exists and is valid, otherwise <c>null</c>.</returns>
        public Guid? GetNullableGuid(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? Guid.TryParse(value.S, out var id)
                    ? id
                    : null
                : null;
        }

        /// <summary>
        ///     Gets a <see cref="Guid" /> value from the attribute dictionary using an exact format
        ///     string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing. Valid formats: "N" (32 digits),
        ///     "D" (hyphens), "B" (braces), "P" (parentheses), "X" (hexadecimal).
        /// </param>
        /// <returns>
        ///     The <see cref="Guid" /> value if the key exists and matches the format, otherwise
        ///     <see cref="Guid.Empty" />.
        /// </returns>
        public Guid GetGuid(string key, string format) =>
            attributes.TryGetValue(key, out var value)
                ? Guid.TryParseExact(value.S, format, out var id)
                    ? id
                    : Guid.Empty
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
        /// <returns>
        ///     The <see cref="Guid" /> value if the key exists and matches the format, otherwise
        ///     <c>null</c>.
        /// </returns>
        public Guid? GetNullableGuid(string key, string format) =>
            attributes.TryGetValue(key, out var value)
                ? Guid.TryParseExact(value.S, format, out var id)
                    ? id
                    : null
                : null;

        /// <summary>Sets a <see cref="Guid" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="Guid" /> value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetGuid(string key, Guid value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable <see cref="Guid" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable <see cref="Guid" /> value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableGuid(string key, Guid? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        /// <summary>Sets a <see cref="Guid" /> value in the attribute dictionary using a specific format.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="Guid" /> value to set.</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string. Valid formats:
        ///     "N", "D", "B", "P", "X".
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetGuid(string key, Guid value, string format)
        {
            attributes[key] = value.ToAttributeValue(format);
            return attributes;
        }

        /// <summary>
        ///     Sets a nullable <see cref="Guid" /> value in the attribute dictionary using a specific
        ///     format.
        /// </summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable <see cref="Guid" /> value to set.</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string. Valid formats:
        ///     "N", "D", "B", "P", "X".
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableGuid(
            string key,
            Guid? value,
            string format
        )
        {
            attributes[key] = value.ToNullableAttributeValue(format);
            return attributes;
        }

        #endregion

        #region DateTime Methods

        /// <summary>
        /// Gets a <see cref="DateTime"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="DateTime"/> value if the key exists and is valid, otherwise <see cref="DateTime.MinValue"/>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/> with <see cref="DateTimeStyles.RoundtripKind"/> for ISO-8601 format.</remarks>
        public DateTime GetDateTime(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? DateTime.TryParse(
                    value.S,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var date
                )
                    ? date
                    : DateTime.MinValue
                : DateTime.MinValue;
        }

        /// <summary>
        ///     Gets a <see cref="DateTime" /> value from the attribute dictionary using an exact format
        ///     string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "yyyy-MM-dd", "yyyyMMdd",
        ///     "MM/dd/yyyy").
        /// </param>
        /// <returns>
        ///     The <see cref="DateTime" /> value if the key exists and matches the format, otherwise
        ///     <see cref="DateTime.MinValue" />.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" />.
        /// </remarks>
        public DateTime GetDateTime(string key, string format) =>
            attributes.TryGetValue(key, out var value)
                ? DateTime.TryParseExact(
                    value.S,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var date
                )
                    ? date
                    : DateTime.MinValue
                : DateTime.MinValue;

        /// <summary>
        ///     Gets a nullable <see cref="DateTime" /> value from the attribute dictionary using an exact
        ///     format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "yyyy-MM-dd", "yyyyMMdd",
        ///     "MM/dd/yyyy").
        /// </param>
        /// <returns>
        ///     The <see cref="DateTime" /> value if the key exists and matches the format, otherwise
        ///     <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" />.
        /// </remarks>
        public DateTime? GetNullableDateTime(string key, string format) =>
            attributes.TryGetValue(key, out var value)
                ? DateTime.TryParseExact(
                    value.S,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var date
                )
                    ? date
                    : null
                : null;

        /// <summary>
        /// Gets a nullable <see cref="DateTime"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="DateTime"/> value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/> with <see cref="DateTimeStyles.RoundtripKind"/> for ISO-8601 format.</remarks>
        public DateTime? GetNullableDateTime(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? DateTime.TryParse(
                    value.S,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var date
                )
                    ? date
                    : null
                : null;
        }

        /// <summary>
        /// Gets a <see cref="DateTimeOffset"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="DateTimeOffset"/> value if the key exists and is valid, otherwise <see cref="DateTimeOffset.MinValue"/>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/> with <see cref="DateTimeStyles.RoundtripKind"/> for ISO-8601 format.</remarks>
        public DateTimeOffset GetDateTimeOffset(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? DateTimeOffset.TryParse(
                    value.S,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var date
                )
                    ? date
                    : DateTimeOffset.MinValue
                : DateTimeOffset.MinValue;
        }

        /// <summary>
        /// Gets a nullable <see cref="DateTimeOffset"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="DateTimeOffset"/> value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/> with <see cref="DateTimeStyles.RoundtripKind"/> for ISO-8601 format.</remarks>
        public DateTimeOffset? GetNullableDateTimeOffset(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? DateTimeOffset.TryParse(
                    value.S,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var date
                )
                    ? date
                    : null
                : null;
        }

        /// <summary>
        ///     Gets a <see cref="DateTimeOffset" /> value from the attribute dictionary using an exact
        ///     format string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "yyyy-MM-dd", "yyyyMMdd",
        ///     "o").
        /// </param>
        /// <returns>
        ///     The <see cref="DateTimeOffset" /> value if the key exists and matches the format,
        ///     otherwise <see cref="DateTimeOffset.MinValue" />.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" />.
        /// </remarks>
        public DateTimeOffset GetDateTimeOffset(string key, string format) =>
            attributes.TryGetValue(key, out var value)
                ? DateTimeOffset.TryParseExact(
                    value.S,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var date
                )
                    ? date
                    : DateTimeOffset.MinValue
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
        /// <returns>
        ///     The <see cref="DateTimeOffset" /> value if the key exists and matches the format,
        ///     otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     Parsing uses <see cref="CultureInfo.InvariantCulture" /> with
        ///     <see cref="DateTimeStyles.RoundtripKind" />.
        /// </remarks>
        public DateTimeOffset? GetNullableDateTimeOffset(string key, string format) =>
            attributes.TryGetValue(key, out var value)
                ? DateTimeOffset.TryParseExact(
                    value.S,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var date
                )
                    ? date
                    : null
                : null;

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="TimeSpan"/> value if the key exists and is valid, otherwise <see cref="TimeSpan.Zero"/>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public TimeSpan GetTimeSpan(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? TimeSpan.TryParse(value.S, CultureInfo.InvariantCulture, out var timeSpan)
                    ? timeSpan
                    : TimeSpan.Zero
                : TimeSpan.Zero;
        }

        /// <summary>
        /// Gets a nullable <see cref="TimeSpan"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="TimeSpan"/> value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public TimeSpan? GetNullableTimeSpan(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? TimeSpan.TryParse(value.S, CultureInfo.InvariantCulture, out var timeSpan)
                    ? timeSpan
                    : null
                : null;
        }

        /// <summary>
        ///     Gets a <see cref="TimeSpan" /> value from the attribute dictionary using an exact format
        ///     string.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">
        ///     The exact format string to use for parsing (e.g., "c", "g", "G", or custom
        ///     patterns like "hh\\:mm\\:ss").
        /// </param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and matches the format, otherwise
        ///     <see cref="TimeSpan.Zero" />.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan GetTimeSpan(string key, string format) =>
            attributes.TryGetValue(key, out var value)
                ? TimeSpan.TryParseExact(
                    value.S,
                    format,
                    CultureInfo.InvariantCulture,
                    out var timeSpan
                )
                    ? timeSpan
                    : TimeSpan.Zero
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
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and matches the format, otherwise
        ///     <c>null</c>.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan? GetNullableTimeSpan(string key, string format) =>
            attributes.TryGetValue(key, out var value)
                ? TimeSpan.TryParseExact(
                    value.S,
                    format,
                    CultureInfo.InvariantCulture,
                    out var timeSpan
                )
                    ? timeSpan
                    : null
                : null;

        /// <summary>Sets a <see cref="DateTime" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateTime" /> value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDateTime(string key, DateTime value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable <see cref="DateTime" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable <see cref="DateTime" /> value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableDateTime(string key, DateTime? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        /// <summary>Sets a <see cref="DateTimeOffset" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateTimeOffset" /> value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDateTimeOffset(
            string key,
            DateTimeOffset value
        )
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable <see cref="DateTimeOffset" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable <see cref="DateTimeOffset" /> value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableDateTimeOffset(
            string key,
            DateTimeOffset? value
        )
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        /// <summary>Sets a <see cref="TimeSpan" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="TimeSpan" /> value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetTimeSpan(string key, TimeSpan value)
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable <see cref="TimeSpan" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable <see cref="TimeSpan" /> value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableTimeSpan(string key, TimeSpan? value)
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        /// <summary>Sets a <see cref="DateTime" /> value in the attribute dictionary using a specific format.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateTime" /> value to set.</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "yyyyMMdd", "o").
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetDateTime(
            string key,
            DateTime value,
            string format
        )
        {
            attributes[key] = value.ToAttributeValue(format);
            return attributes;
        }

        /// <summary>
        ///     Sets a nullable <see cref="DateTime" /> value in the attribute dictionary using a specific
        ///     format.
        /// </summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable <see cref="DateTime" /> value to set.</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "yyyyMMdd", "o").
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetNullableDateTime(
            string key,
            DateTime? value,
            string format
        )
        {
            attributes[key] = value.ToNullableAttributeValue(format);
            return attributes;
        }

        /// <summary>
        ///     Sets a <see cref="DateTimeOffset" /> value in the attribute dictionary using a specific
        ///     format.
        /// </summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateTimeOffset" /> value to set.</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "o").
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetDateTimeOffset(
            string key,
            DateTimeOffset value,
            string format
        )
        {
            attributes[key] = value.ToAttributeValue(format);
            return attributes;
        }

        /// <summary>
        ///     Sets a nullable <see cref="DateTimeOffset" /> value in the attribute dictionary using a
        ///     specific format.
        /// </summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable <see cref="DateTimeOffset" /> value to set.</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "o").
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetNullableDateTimeOffset(
            string key,
            DateTimeOffset? value,
            string format
        )
        {
            attributes[key] = value.ToNullableAttributeValue(format);
            return attributes;
        }

        /// <summary>Sets a <see cref="TimeSpan" /> value in the attribute dictionary using a specific format.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="TimeSpan" /> value to set.</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g., "c",
        ///     "g", "G").
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetTimeSpan(
            string key,
            TimeSpan value,
            string format
        )
        {
            attributes[key] = value.ToAttributeValue(format);
            return attributes;
        }

        /// <summary>
        ///     Sets a nullable <see cref="TimeSpan" /> value in the attribute dictionary using a specific
        ///     format.
        /// </summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable <see cref="TimeSpan" /> value to set.</param>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g., "c",
        ///     "g", "G").
        /// </param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetNullableTimeSpan(
            string key,
            TimeSpan? value,
            string format
        )
        {
            attributes[key] = value.ToNullableAttributeValue(format);
            return attributes;
        }

        #endregion

        #region Enum Methods

        /// <summary>
        /// Gets an enum value from the attribute dictionary.
        /// </summary>
        /// <typeparam name="TEnum">The enum type to parse.</typeparam>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the key doesn't exist or parsing fails.</param>
        /// <returns>The enum value if the key exists and is valid, otherwise <paramref name="defaultValue"/>.</returns>
        public TEnum GetEnum<TEnum>(string key, TEnum defaultValue)
            where TEnum : struct
        {
            if (
                attributes.TryGetValue(key, out var value)
                && Enum.TryParse(value.S, out TEnum valueEnum)
            )
            {
                return valueEnum;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a nullable enum value from the attribute dictionary.
        /// </summary>
        /// <typeparam name="TEnum">The enum type to parse.</typeparam>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The enum value if the key exists and is valid, otherwise <c>null</c>.</returns>
        public TEnum? GetNullableEnum<TEnum>(string key)
            where TEnum : struct
        {
            if (
                attributes.TryGetValue(key, out var value)
                && Enum.TryParse(value.S, out TEnum valueEnum)
            )
            {
                return valueEnum;
            }

            return null;
        }

        /// <summary>Sets an enum value in the attribute dictionary.</summary>
        /// <typeparam name="TEnum">The enum type to set.</typeparam>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The enum value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetEnum<TEnum>(string key, TEnum value)
            where TEnum : struct, Enum
        {
            attributes[key] = value.ToAttributeValue();
            return attributes;
        }

        /// <summary>Sets a nullable enum value in the attribute dictionary.</summary>
        /// <typeparam name="TEnum">The enum type to set.</typeparam>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The nullable enum value to set.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetNullableEnum<TEnum>(string key, TEnum? value)
            where TEnum : struct, Enum
        {
            attributes[key] = value.ToNullableAttributeValue();
            return attributes;
        }

        #endregion
    }

    #region String Extensions

    extension(string value)
    {
        /// <summary>Converts a string value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>An <see cref="AttributeValue" /> containing the string value.</returns>
        public AttributeValue ToAttributeValue() => new() { S = value };
    }

    extension(string? value)
    {
        /// <summary>Converts a nullable string value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the string value, or a NULL attribute if the
        ///     value is <c>null</c>.
        /// </returns>
        public AttributeValue ToNullableAttributeValue() =>
            value is null ? new AttributeValue { NULL = true } : new AttributeValue { S = value };
    }

    #endregion

    #region Boolean Extensions

    extension(bool value)
    {
        /// <summary>Converts a boolean value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>An <see cref="AttributeValue" /> containing the boolean value.</returns>
        public AttributeValue ToAttributeValue() => new() { BOOL = value };
    }

    extension(bool? value)
    {
        /// <summary>Converts a nullable boolean value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the boolean value, or a NULL attribute if the
        ///     value is <c>null</c>.
        /// </returns>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { BOOL = value };
    }

    #endregion

    #region Integer Extensions

    extension(int value)
    {
        /// <summary>Converts an integer value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>An <see cref="AttributeValue" /> containing the numeric value.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToAttributeValue() =>
            new() { N = value.ToString(CultureInfo.InvariantCulture) };
    }

    extension(int? value)
    {
        /// <summary>Converts a nullable integer value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the numeric value, or a NULL attribute if the
        ///     value is <c>null</c>.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { N = value.Value.ToString(CultureInfo.InvariantCulture) };
    }

    extension(long value)
    {
        /// <summary>Converts a long integer value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>An <see cref="AttributeValue" /> containing the numeric value.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToAttributeValue() =>
            new() { N = value.ToString(CultureInfo.InvariantCulture) };
    }

    extension(long? value)
    {
        /// <summary>Converts a nullable long integer value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the numeric value, or a NULL attribute if the
        ///     value is <c>null</c>.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { N = value.Value.ToString(CultureInfo.InvariantCulture) };
    }

    #endregion

    #region Floating Point Extensions

    extension(float value)
    {
        /// <summary>
        ///     Converts a single-precision floating-point value to a DynamoDB
        ///     <see cref="AttributeValue" />.
        /// </summary>
        /// <returns>An <see cref="AttributeValue" /> containing the numeric value.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToAttributeValue() =>
            new() { N = value.ToString(CultureInfo.InvariantCulture) };
    }

    extension(float? value)
    {
        /// <summary>
        ///     Converts a nullable single-precision floating-point value to a DynamoDB
        ///     <see cref="AttributeValue" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the numeric value, or a NULL attribute if the
        ///     value is <c>null</c>.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { N = value.Value.ToString(CultureInfo.InvariantCulture) };
    }

    extension(double value)
    {
        /// <summary>
        ///     Converts a double-precision floating-point value to a DynamoDB
        ///     <see cref="AttributeValue" />.
        /// </summary>
        /// <returns>An <see cref="AttributeValue" /> containing the numeric value.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToAttributeValue() =>
            new() { N = value.ToString(CultureInfo.InvariantCulture) };
    }

    extension(double? value)
    {
        /// <summary>
        ///     Converts a nullable double-precision floating-point value to a DynamoDB
        ///     <see cref="AttributeValue" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the numeric value, or a NULL attribute if the
        ///     value is <c>null</c>.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { N = value.Value.ToString(CultureInfo.InvariantCulture) };
    }

    extension(decimal value)
    {
        /// <summary>Converts a decimal value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>An <see cref="AttributeValue" /> containing the numeric value.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToAttributeValue() =>
            new() { N = value.ToString(CultureInfo.InvariantCulture) };
    }

    extension(decimal? value)
    {
        /// <summary>Converts a nullable decimal value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the numeric value, or a NULL attribute if the
        ///     value is <c>null</c>.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { N = value.Value.ToString(CultureInfo.InvariantCulture) };
    }

    #endregion

    #region Guid Extensions

    extension(Guid value)
    {
        /// <summary>Converts a <see cref="Guid" /> value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>An <see cref="AttributeValue" /> containing the <see cref="Guid" /> as a string.</returns>
        public AttributeValue ToAttributeValue() => new() { S = value.ToString() };

        /// <summary>
        ///     Converts a <see cref="Guid" /> value to a DynamoDB <see cref="AttributeValue" /> using a
        ///     specific format string.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when converting the value to a string. Valid formats:
        ///     "N", "D", "B", "P", "X".
        /// </param>
        /// <returns>An <see cref="AttributeValue" /> containing the <see cref="Guid" /> as a string.</returns>
        public AttributeValue ToAttributeValue(string format) =>
            new() { S = value.ToString(format) };
    }

    extension(Guid? value)
    {
        /// <summary>Converts a nullable <see cref="Guid" /> value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="Guid" /> as a string, or a NULL
        ///     attribute if the value is <c>null</c>.
        /// </returns>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { S = value.Value.ToString() };

        /// <summary>
        ///     Converts a nullable <see cref="Guid" /> value to a DynamoDB <see cref="AttributeValue" />
        ///     using a specific format string.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when converting the value to a string. Valid formats:
        ///     "N", "D", "B", "P", "X".
        /// </param>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="Guid" /> as a string, or a NULL
        ///     attribute if the value is <c>null</c>.
        /// </returns>
        public AttributeValue ToNullableAttributeValue(string format) =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { S = value.Value.ToString(format) };
    }

    #endregion

    #region DateTime Extensions

    extension(DateTime value)
    {
        /// <summary>Converts a <see cref="DateTime" /> value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="DateTime" /> in ISO-8601
        ///     format.
        /// </returns>
        /// <remarks>
        ///     Formatting uses <see cref="CultureInfo.InvariantCulture" /> with round-trip format
        ///     specifier.
        /// </remarks>
        public AttributeValue ToAttributeValue() =>
            new() { S = value.ToString("o", CultureInfo.InvariantCulture) };

        /// <summary>
        ///     Converts a <see cref="DateTime" /> value to a DynamoDB <see cref="AttributeValue" /> using
        ///     a specific format string.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "yyyyMMdd", "o").
        /// </param>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="DateTime" /> in the specified
        ///     format.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToAttributeValue(string format) =>
            new() { S = value.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(DateTime? value)
    {
        /// <summary>
        ///     Converts a nullable <see cref="DateTime" /> value to a DynamoDB
        ///     <see cref="AttributeValue" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="DateTime" /> in ISO-8601
        ///     format, or a NULL attribute if the value is <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     Formatting uses <see cref="CultureInfo.InvariantCulture" /> with round-trip format
        ///     specifier.
        /// </remarks>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString("o", CultureInfo.InvariantCulture),
                };

        /// <summary>
        ///     Converts a nullable <see cref="DateTime" /> value to a DynamoDB
        ///     <see cref="AttributeValue" /> using a specific format string.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "yyyyMMdd", "o").
        /// </param>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="DateTime" /> in the specified
        ///     format, or a NULL attribute if the value is <c>null</c>.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToNullableAttributeValue(string format) =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(DateTimeOffset value)
    {
        /// <summary>
        ///     Converts a <see cref="DateTimeOffset" /> value to a DynamoDB <see cref="AttributeValue" />
        ///     .
        /// </summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="DateTimeOffset" /> in ISO-8601
        ///     format.
        /// </returns>
        /// <remarks>
        ///     Formatting uses <see cref="CultureInfo.InvariantCulture" /> with round-trip format
        ///     specifier.
        /// </remarks>
        public AttributeValue ToAttributeValue() =>
            new() { S = value.ToString("o", CultureInfo.InvariantCulture) };

        /// <summary>
        ///     Converts a <see cref="DateTimeOffset" /> value to a DynamoDB <see cref="AttributeValue" />
        ///     using a specific format string.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "o").
        /// </param>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="DateTimeOffset" /> in the
        ///     specified format.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToAttributeValue(string format) =>
            new() { S = value.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(DateTimeOffset? value)
    {
        /// <summary>
        ///     Converts a nullable <see cref="DateTimeOffset" /> value to a DynamoDB
        ///     <see cref="AttributeValue" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="DateTimeOffset" /> in ISO-8601
        ///     format, or a NULL attribute if the value is <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     Formatting uses <see cref="CultureInfo.InvariantCulture" /> with round-trip format
        ///     specifier.
        /// </remarks>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString("o", CultureInfo.InvariantCulture),
                };

        /// <summary>
        ///     Converts a nullable <see cref="DateTimeOffset" /> value to a DynamoDB
        ///     <see cref="AttributeValue" /> using a specific format string.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g.,
        ///     "yyyy-MM-dd", "o").
        /// </param>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="DateTimeOffset" /> in the
        ///     specified format, or a NULL attribute if the value is <c>null</c>.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToNullableAttributeValue(string format) =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(TimeSpan value)
    {
        /// <summary>Converts a <see cref="TimeSpan" /> value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="TimeSpan" /> in constant
        ///     format.
        /// </returns>
        /// <remarks>
        ///     Formatting uses <see cref="CultureInfo.InvariantCulture" /> with constant format
        ///     specifier.
        /// </remarks>
        public AttributeValue ToAttributeValue() =>
            new() { S = value.ToString("c", CultureInfo.InvariantCulture) };

        /// <summary>
        ///     Converts a <see cref="TimeSpan" /> value to a DynamoDB <see cref="AttributeValue" /> using
        ///     a specific format string.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g., "c",
        ///     "g", "G").
        /// </param>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="TimeSpan" /> in the specified
        ///     format.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToAttributeValue(string format) =>
            new() { S = value.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(TimeSpan? value)
    {
        /// <summary>
        ///     Converts a nullable <see cref="TimeSpan" /> value to a DynamoDB
        ///     <see cref="AttributeValue" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="TimeSpan" /> in constant
        ///     format, or a NULL attribute if the value is <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     Formatting uses <see cref="CultureInfo.InvariantCulture" /> with constant format
        ///     specifier.
        /// </remarks>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString("c", CultureInfo.InvariantCulture),
                };

        /// <summary>
        ///     Converts a nullable <see cref="TimeSpan" /> value to a DynamoDB
        ///     <see cref="AttributeValue" /> using a specific format string.
        /// </summary>
        /// <param name="format">
        ///     The format string to use when converting the value to a string (e.g., "c",
        ///     "g", "G").
        /// </param>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the <see cref="TimeSpan" /> in the specified
        ///     format, or a NULL attribute if the value is <c>null</c>.
        /// </returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public AttributeValue ToNullableAttributeValue(string format) =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    #endregion

    #region Enum Extensions

    extension<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        /// <summary>Converts an enum value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>An <see cref="AttributeValue" /> containing the enum value as a string.</returns>
        public AttributeValue ToAttributeValue() => new() { S = value.ToString() };
    }

    extension<TEnum>(TEnum? value)
        where TEnum : struct, Enum
    {
        /// <summary>Converts a nullable enum value to a DynamoDB <see cref="AttributeValue" />.</summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>
        ///     An <see cref="AttributeValue" /> containing the enum value as a string, or a NULL
        ///     attribute if the value is <c>null</c>.
        /// </returns>
        public AttributeValue ToNullableAttributeValue() =>
            value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { S = value.Value.ToString() };
    }

    #endregion
}
