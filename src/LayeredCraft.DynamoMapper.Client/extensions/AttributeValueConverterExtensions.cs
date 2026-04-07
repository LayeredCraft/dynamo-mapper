using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Runtime;

/// <summary>
///     Convenience extensions for converting common scalar values to
///     <see cref="AttributeValue" />.
/// </summary>
public static class AttributeValueConverterExtensions
{
    extension(string? str)
    {
        /// <summary>Converts the string to a DynamoDB string attribute or NULL attribute.</summary>
        public AttributeValue ToAttributeValue()
            => str is null ? new AttributeValue { NULL = true } : new AttributeValue { S = str };
    }

    extension(bool value)
    {
        /// <summary>Converts the boolean to a DynamoDB BOOL attribute.</summary>
        public AttributeValue ToAttributeValue() => new() { BOOL = value };
    }

    extension(bool? value)
    {
        /// <summary>Converts the nullable boolean to a DynamoDB BOOL attribute or NULL attribute.</summary>
        public AttributeValue ToAttributeValue()
            => value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { BOOL = value.Value };
    }

    extension(int num)
    {
        /// <summary>Converts the integer to a DynamoDB number attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => new() { N = num.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(int? num)
    {
        /// <summary>Converts the nullable integer to a DynamoDB number attribute or NULL attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => num is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    N = num.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(long num)
    {
        /// <summary>Converts the long integer to a DynamoDB number attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => new() { N = num.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(long? num)
    {
        /// <summary>Converts the nullable long integer to a DynamoDB number attribute or NULL attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => num is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    N = num.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(float num)
    {
        /// <summary>Converts the single-precision number to a DynamoDB number attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => new() { N = num.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(float? num)
    {
        /// <summary>
        ///     Converts the nullable single-precision number to a DynamoDB number attribute or NULL
        ///     attribute.
        /// </summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => num is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    N = num.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(double num)
    {
        /// <summary>Converts the double-precision number to a DynamoDB number attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => new() { N = num.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(double? num)
    {
        /// <summary>
        ///     Converts the nullable double-precision number to a DynamoDB number attribute or NULL
        ///     attribute.
        /// </summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => num is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    N = num.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(decimal num)
    {
        /// <summary>Converts the decimal number to a DynamoDB number attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => new() { N = num.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(decimal? num)
    {
        /// <summary>Converts the nullable decimal number to a DynamoDB number attribute or NULL attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax("NumericFormat")] string? format = null)
            => num is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    N = num.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(Guid value)
    {
        /// <summary>Converts the GUID to a DynamoDB string attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax(StringSyntaxAttribute.GuidFormat)] string format = "D")
            => new() { S = value.ToString(format) };
    }

    extension(Guid? value)
    {
        /// <summary>Converts the nullable GUID to a DynamoDB string attribute or NULL attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax(StringSyntaxAttribute.GuidFormat)] string format = "D")
            => value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { S = value.Value.ToString(format) };
    }

    extension(DateTime value)
    {
        /// <summary>Converts the date and time to a DynamoDB string attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string format = "o")
            => new() { S = value.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(DateTime? value)
    {
        /// <summary>Converts the nullable date and time to a DynamoDB string attribute or NULL attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string format = "o")
            => value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(DateTimeOffset value)
    {
        /// <summary>Converts the date and time offset to a DynamoDB string attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string format = "o")
            => new() { S = value.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(DateTimeOffset? value)
    {
        /// <summary>
        ///     Converts the nullable date and time offset to a DynamoDB string attribute or NULL
        ///     attribute.
        /// </summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string format = "o")
            => value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }

    extension(TimeSpan value)
    {
        /// <summary>Converts the time span to a DynamoDB string attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] string format = "c")
            => new() { S = value.ToString(format, CultureInfo.InvariantCulture) };
    }

    extension(TimeSpan? value)
    {
        /// <summary>Converts the nullable time span to a DynamoDB string attribute or NULL attribute.</summary>
        public AttributeValue ToAttributeValue(
            [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] string format = "c")
            => value is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = value.Value.ToString(format, CultureInfo.InvariantCulture),
                };
    }
}
