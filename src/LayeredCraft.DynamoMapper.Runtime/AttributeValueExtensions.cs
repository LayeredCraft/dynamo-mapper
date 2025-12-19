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
        public string GetString(string key)
        {
            return attributes.TryGetValue(key, out var value) ? value.S ?? string.Empty : string.Empty;
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
                ? int.TryParse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0
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
                ? int.TryParse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null
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
                ? long.TryParse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0
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
                ? long.TryParse(value.N, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null
                : null;
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
                ? float.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0f
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
                ? float.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null
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
                ? double.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0.0
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
                ? double.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null
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
                ? decimal.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0m
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
                ? decimal.TryParse(value.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null
                : null;
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
                ? Guid.TryParse(value.S, out var id) ? id : Guid.Empty
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
                ? Guid.TryParse(value.S, out var id) ? id : null
                : null;
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
                ? DateTime.TryParse(value.S, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date) ? date : DateTime.MinValue
                : DateTime.MinValue;
        }

        /// <summary>
        /// Gets a nullable <see cref="DateTime"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="DateTime"/> value if the key exists and is valid, otherwise <c>null</c>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/> with <see cref="DateTimeStyles.RoundtripKind"/> for ISO-8601 format.</remarks>
        public DateTime? GetNullableDateTime(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? DateTime.TryParse(value.S, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date) ? date : null
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
                ? DateTimeOffset.TryParse(value.S, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date) ? date : DateTimeOffset.MinValue
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
                ? DateTimeOffset.TryParse(value.S, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date) ? date : null
                : null;
        }

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> value from the attribute dictionary.
        /// </summary>
        /// <param name="key">The attribute key to retrieve.</param>
        /// <returns>The <see cref="TimeSpan"/> value if the key exists and is valid, otherwise <see cref="TimeSpan.Zero"/>.</returns>
        /// <remarks>Parsing uses <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        public TimeSpan GetTimeSpan(string key)
        {
            return attributes.TryGetValue(key, out var value)
                ? TimeSpan.TryParse(value.S, CultureInfo.InvariantCulture, out var timeSpan) ? timeSpan : TimeSpan.Zero
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
                ? TimeSpan.TryParse(value.S, CultureInfo.InvariantCulture, out var timeSpan) ? timeSpan : null
                : null;
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
        public TEnum GetEnum<TEnum>(string key, TEnum defaultValue) where TEnum : struct
        {
            if (attributes.TryGetValue(key, out var value) && Enum.TryParse(value.S, out TEnum valueEnum))
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
        public TEnum? GetNullableEnum<TEnum>(string key) where TEnum : struct
        {
            if (attributes.TryGetValue(key, out var value) && Enum.TryParse(value.S, out TEnum valueEnum))
            {
                return valueEnum;
            }

            return null;
        }

        #endregion
    }
}