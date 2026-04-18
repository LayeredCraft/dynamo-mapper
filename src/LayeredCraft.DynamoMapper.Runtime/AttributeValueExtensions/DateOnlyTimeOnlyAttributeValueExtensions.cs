#if NET6_0_OR_GREATER
using System.Globalization;
using LayeredCraft.DynamoMapper.Runtime;

namespace Amazon.DynamoDBv2.Model;

/// <summary>
///     Extension methods for <see cref="Dictionary{TKey, TValue}" /> of <see cref="string" /> and
///     <see cref="AttributeValue" /> for <see cref="DateOnly" /> and <see cref="TimeOnly" /> values.
/// </summary>
public static class DateOnlyTimeOnlyAttributeValueExtensions
{
    extension(Dictionary<string, AttributeValue> attributes)
    {
        // ==================== DateOnly ====================

        /// <summary>Tries to get a <see cref="DateOnly" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The <see cref="DateOnly" /> value when found.</param>
        /// <param name="format">The exact format string to use for parsing. Default is "yyyy-MM-dd".</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetDateOnly(
            string key,
            out DateOnly value,
            string format = "yyyy-MM-dd",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            value = DateOnly.MinValue;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetString(kind);
            value = stringValue.Length == 0
                ? DateOnly.MinValue
                : DateOnly.ParseExact(stringValue, format, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a <see cref="DateOnly" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">The exact format string to use for parsing. Default is "yyyy-MM-dd".</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="DateOnly" /> value if the key exists; otherwise <see cref="DateOnly.MinValue" />.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public DateOnly GetDateOnly(
            string key,
            string format = "yyyy-MM-dd",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        ) =>
            attributes.TryGetDateOnly(key, out var value, format, requiredness, kind)
                ? value
                : DateOnly.MinValue;

        /// <summary>Tries to get a nullable <see cref="DateOnly" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable <see cref="DateOnly" /> value when found.</param>
        /// <param name="format">The exact format string to use for parsing. Default is "yyyy-MM-dd".</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetNullableDateOnly(
            string key,
            out DateOnly? value,
            string format = "yyyy-MM-dd",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            if (attribute.IsNull)
                return true;

            var stringValue = attribute!.GetNullableString(kind);
            value = stringValue is null
                ? null
                : DateOnly.ParseExact(stringValue, format, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a nullable <see cref="DateOnly" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">The exact format string to use for parsing. Default is "yyyy-MM-dd".</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="DateOnly" /> value if the key exists; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public DateOnly? GetNullableDateOnly(
            string key,
            string format = "yyyy-MM-dd",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        ) =>
            attributes.TryGetNullableDateOnly(key, out var value, format, requiredness, kind)
                ? value
                : null;

        /// <summary>Sets a <see cref="DateOnly" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateOnly" /> value to set (can be null).</param>
        /// <param name="omitEmptyStrings">Whether to omit empty string values. Default is <c>false</c>.</param>
        /// <param name="omitNullStrings">Whether to omit null string values. Default is <c>true</c>.</param>
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetDateOnly(
            string key,
            DateOnly? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

            return attributes;
        }

        /// <summary>Sets a <see cref="DateOnly" /> value in the attribute dictionary using a specific format.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="DateOnly" /> value to set (can be null).</param>
        /// <param name="format">The format string to use when converting the value to a string.</param>
        /// <param name="omitEmptyStrings">Whether to omit empty string values. Default is <c>false</c>.</param>
        /// <param name="omitNullStrings">Whether to omit null string values. Default is <c>true</c>.</param>
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetDateOnly(
            string key,
            DateOnly? value,
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

        // ==================== TimeOnly ====================

        /// <summary>Tries to get a <see cref="TimeOnly" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The <see cref="TimeOnly" /> value when found.</param>
        /// <param name="format">The exact format string to use for parsing. Default is "HH:mm:ss.fffffff".</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetTimeOnly(
            string key,
            out TimeOnly value,
            string format = "HH:mm:ss.fffffff",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            value = TimeOnly.MinValue;
            if (!attributes.TryGetValue(key, requiredness, out var attribute))
                return false;

            var stringValue = attribute!.GetString(kind);
            value = stringValue.Length == 0
                ? TimeOnly.MinValue
                : TimeOnly.ParseExact(stringValue, format, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a <see cref="TimeOnly" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">The exact format string to use for parsing. Default is "HH:mm:ss.fffffff".</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="TimeOnly" /> value if the key exists; otherwise <see cref="TimeOnly.MinValue" />.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeOnly GetTimeOnly(
            string key,
            string format = "HH:mm:ss.fffffff",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        ) =>
            attributes.TryGetTimeOnly(key, out var value, format, requiredness, kind)
                ? value
                : TimeOnly.MinValue;

        /// <summary>Tries to get a nullable <see cref="TimeOnly" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="value">The nullable <see cref="TimeOnly" /> value when found.</param>
        /// <param name="format">The exact format string to use for parsing. Default is "HH:mm:ss.fffffff".</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns><c>true</c> when the key exists and the value is retrieved; otherwise <c>false</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public bool TryGetNullableTimeOnly(
            string key,
            out TimeOnly? value,
            string format = "HH:mm:ss.fffffff",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        )
        {
            value = null;
            if (!attributes.TryGetNullableValue(key, requiredness, out var attribute))
                return false;

            if (attribute.IsNull)
                return true;

            var stringValue = attribute!.GetNullableString(kind);
            value = stringValue is null
                ? null
                : TimeOnly.ParseExact(stringValue, format, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Gets a nullable <see cref="TimeOnly" /> value from the attribute dictionary.</summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <param name="format">The exact format string to use for parsing. Default is "HH:mm:ss.fffffff".</param>
        /// <param name="requiredness">Specifies whether the attribute is required.</param>
        /// <param name="kind">The DynamoDB attribute kind to interpret as a string.</param>
        /// <returns>
        ///     The <see cref="TimeOnly" /> value if the key exists; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public TimeOnly? GetNullableTimeOnly(
            string key,
            string format = "HH:mm:ss.fffffff",
            Requiredness requiredness = Requiredness.InferFromNullability,
            DynamoKind kind = DynamoKind.S
        ) =>
            attributes.TryGetNullableTimeOnly(key, out var value, format, requiredness, kind)
                ? value
                : null;

        /// <summary>Sets a <see cref="TimeOnly" /> value in the attribute dictionary.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="TimeOnly" /> value to set (can be null).</param>
        /// <param name="omitEmptyStrings">Whether to omit empty string values. Default is <c>false</c>.</param>
        /// <param name="omitNullStrings">Whether to omit null string values. Default is <c>true</c>.</param>
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        public Dictionary<string, AttributeValue> SetTimeOnly(
            string key,
            TimeOnly? value,
            bool omitEmptyStrings = false,
            bool omitNullStrings = true,
            DynamoKind kind = DynamoKind.S
        )
        {
            var stringValue = value?.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            if (stringValue.ShouldSet(omitEmptyStrings, omitNullStrings))
                attributes[key] = stringValue.ToAttributeValue(kind);

            return attributes;
        }

        /// <summary>Sets a <see cref="TimeOnly" /> value in the attribute dictionary using a specific format.</summary>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The <see cref="TimeOnly" /> value to set (can be null).</param>
        /// <param name="format">The format string to use when converting the value to a string.</param>
        /// <param name="omitEmptyStrings">Whether to omit empty string values. Default is <c>false</c>.</param>
        /// <param name="omitNullStrings">Whether to omit null string values. Default is <c>true</c>.</param>
        /// <param name="kind">The DynamoDB attribute kind to write. Default is <see cref="DynamoKind.S" />.</param>
        /// <returns>The attribute dictionary for fluent chaining.</returns>
        /// <remarks>Formatting uses <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public Dictionary<string, AttributeValue> SetTimeOnly(
            string key,
            TimeOnly? value,
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
#endif
