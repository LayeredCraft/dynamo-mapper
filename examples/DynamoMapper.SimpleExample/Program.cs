using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

// Example enum definitions
var exampleAttributes = new Dictionary<string, AttributeValue>
{
    // bool
    ["bool"] = new() { BOOL = true },

    // bool?
    ["nullable_bool"] = new() { BOOL = false },

    // DateTime
    ["date_time"] = new() { S = "2025-12-22T10:30:00.0000000Z" },

    // DateTime?
    ["nullable_date_time"] = new() { S = "2025-12-20T08:00:00.0000000Z" },

    // DateTimeOffset
    ["date_time_offset"] = new() { S = "2025-12-22T10:30:00.0000000+00:00" },

    // DateTimeOffset?
    ["nullable_date_time_offset"] = new() { S = "2025-12-22T12:00:00.0000000+01:00" },

    // decimal
    ["decimal"] = new() { N = "1234567.89" },

    // decimal?
    ["nullable_decimal"] = new() { N = "999.99" },

    // double
    ["double"] = new() { N = "19.99" },

    // double?
    ["nullable_double"] = new() { N = "3.14159" },

    // Guid
    ["guid"] = new() { S = "a1b2c3d4-e5f6-7890-abcd-ef1234567890" },

    // Guid?
    ["nullable_guid"] = new() { S = "12345678-1234-1234-1234-123456789012" },

    // int
    ["int"] = new() { N = "25" },

    // int?
    ["nullable_int"] = new() { N = "100" },

    // long
    ["long"] = new() { N = "9223372036854775807" },

    // long?
    ["nullable_long"] = new() { N = "123456789012345" },

    // string
    ["string"] = new() { S = "John Doe" },

    // string?
    ["nullable_string"] = new() { S = "Optional text" },

    // float
    ["float"] = new() { N = "3.14" },

    // float?
    ["nullable_float"] = new() { N = "2.71" },

    // TimeSpan
    ["time_span"] = new() { S = "PT2H30M45S" },

    // TimeSpan?
    ["nullable_time_span"] = new() { S = "PT5H" },

    // OrderStatus enum
    ["enum"] = new() { S = "Pending" },

    // OrderStatus? enum
    ["nullable_enum"] = new() { S = "Shipped" },
};

var myEntity = ExampleModelMapper.FromItem(exampleAttributes);

[DynamoMapper(
    Convention = DynamoNamingConvention.SnakeCase,
    DefaultRequiredness = Requiredness.Required,
    OmitNullStrings = false,
    OmitEmptyStrings = true,
    DateTimeFormat = "I",
    EnumFormat = "G"
)]
internal static partial class ExampleModelMapper
{
    private static partial Dictionary<string, AttributeValue> ToItem(ExampleModel source);

    internal static partial ExampleModel FromItem(Dictionary<string, AttributeValue> item);
}

internal class ExampleModel
{
    internal bool Bool { get; set; }
    internal bool? NullableBool { get; set; }
    internal DateTime DateTime { get; set; }
    internal DateTime? NullableDateTime { get; set; }
    internal DateTimeOffset DateTimeOffset { get; set; }
    internal DateTimeOffset? NullableDateTimeOffset { get; set; }
    internal decimal Decimal { get; set; }
    internal decimal? NullableDecimal { get; set; }
    internal double Double { get; set; }
    internal double? NullableDouble { get; set; }
    internal Guid Guid { get; set; }
    internal Guid? NullableGuid { get; set; }
    internal int Int { get; set; }
    internal int? NullableInt { get; set; }
    internal long Long { get; set; }
    internal long? NullableLong { get; set; }
    internal required string String { get; set; }
    internal string? NullableString { get; set; }
    internal float Float { get; set; }
    internal float? NullableFloat { get; set; }
    internal TimeSpan TimeSpan { get; set; }
    internal TimeSpan? NullableTimeSpan { get; set; }
    internal OrderStatus Enum { get; set; }
    internal OrderStatus? NullableEnum { get; set; }
}

internal enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
}
