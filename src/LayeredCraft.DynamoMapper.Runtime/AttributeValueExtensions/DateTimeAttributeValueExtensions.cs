using System.Globalization;
using DynamoMapper.Runtime;

namespace Amazon.DynamoDBv2.Model;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> for date and time values.
/// </summary>
public static class DateTimeAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        /// <summary>Gets a <see cref="DateTime" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
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
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes.GetValue(key, requiredness).GetString(kind);
            return stringValue.Length == 0
                ? DateTime.MinValue
                : DateTime.Parse(
                    stringValue,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                );
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
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
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
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes.GetValue(key, requiredness).GetString(kind);
            return stringValue.Length == 0
                ? DateTime.MinValue
                : DateTime.ParseExact(
                    stringValue,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                );
        }

        /// <summary>Gets a nullable <see cref="DateTime" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
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
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes
                .GetNullableValue(key, requiredness)
                .GetNullableString(kind);
            return stringValue is null
                ? null
                : DateTime.Parse(
                    stringValue,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                );
        }

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
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
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
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes
                .GetNullableValue(key, requiredness)
                .GetNullableString(kind);
            return stringValue is null
                ? null
                : DateTime.ParseExact(
                    stringValue,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                );
        }

        /// <summary>Gets a <see cref="DateTimeOffset" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
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
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes.GetValue(key, requiredness).GetString(kind);
            return stringValue.Length == 0
                ? DateTimeOffset.MinValue
                : DateTimeOffset.Parse(
                    stringValue,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                );
        }

        /// <summary>Gets a nullable <see cref="DateTimeOffset" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
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
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes
                .GetNullableValue(key, requiredness)
                .GetNullableString(kind);
            return stringValue is null
                ? null
                : DateTimeOffset.Parse(
                    stringValue,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                );
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
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
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
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes.GetValue(key, requiredness).GetString(kind);
            return stringValue.Length == 0
                ? DateTimeOffset.MinValue
                : DateTimeOffset.ParseExact(
                    stringValue,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                );
        }

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
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
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
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes
                .GetNullableValue(key, requiredness)
                .GetNullableString(kind);
            return stringValue is null
                ? null
                : DateTimeOffset.ParseExact(
                    stringValue,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind
                );
        }

        /// <summary>Gets a <see cref="TimeSpan" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and is valid; otherwise
        ///     <see cref="TimeSpan.Zero" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan GetTimeSpan(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes.GetValue(key, requiredness).GetString(kind);
            return stringValue.Length == 0
                ? TimeSpan.Zero
                : TimeSpan.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        /// <summary>Gets a nullable <see cref="TimeSpan" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and is valid; otherwise <c>null</c> if
        ///     the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan? GetNullableTimeSpan(
            string key,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes
                .GetNullableValue(key, requiredness)
                .GetNullableString(kind);
            return stringValue is null
                ? null
                : TimeSpan.Parse(stringValue, CultureInfo.InvariantCulture);
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
        /// <param name="requiredness">
        ///     Specifies whether the attribute is required. Default is
        ///     <see cref="Requiredness.InferFromNullability" />.
        /// </param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and matches the format; otherwise
        ///     <see cref="TimeSpan.Zero" /> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan GetTimeSpan(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes.GetValue(key, requiredness).GetString(kind);
            return stringValue.Length == 0
                ? TimeSpan.Zero
                : TimeSpan.ParseExact(stringValue, format, CultureInfo.InvariantCulture);
        }

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
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="TimeSpan" /> value if the key exists and matches the format; otherwise
        ///     <c>null</c> if the key is missing or the attribute has a DynamoDB NULL value.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeSpan? GetNullableTimeSpan(
            string key,
            string format,
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = attributes
                .GetNullableValue(key, requiredness)
                .GetNullableString(kind);
            return stringValue is null
                ? null
                : TimeSpan.ParseExact(stringValue, format, CultureInfo.InvariantCulture);
        }

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDateTime(
            string key,
            DateTime? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString("o", CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDateTimeOffset(
            string key,
            DateTimeOffset? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString("o", CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetTimeSpan(
            string key,
            TimeSpan? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString("c", CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetDateTime(
            string key,
            DateTime? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString(format, CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetDateTimeOffset(
            string key,
            DateTimeOffset? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString(format, CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

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
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetTimeSpan(
            string key,
            TimeSpan? value,
            string format,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString(format, CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

            return attributes;
        }
    }
}
