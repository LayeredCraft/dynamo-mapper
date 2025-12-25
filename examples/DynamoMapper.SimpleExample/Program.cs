using System;
using System.Collections.Generic;
using System.Globalization;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

// Example enum definitions
var exampleAttributes = new Dictionary<string, AttributeValue>
{
    // String
    ["Name"] = new() { S = "John Doe" },
    ["EmptyString"] = new() { S = "" },

    // Boolean
    ["IsActive"] = new() { BOOL = true },
    ["IsDeleted"] = new() { BOOL = false },

    // Nullable Boolean
    ["IsVerified"] = new() { BOOL = true },
    ["IsOptional"] = new() { NULL = true }, // null bool?

    // Integer types
    ["Age"] = new() { N = "25" },
    ["Count"] = new() { N = "0" },
    ["NegativeInt"] = new() { N = "-42" },

    // Nullable int
    ["OptionalCount"] = new() { N = "100" },
    ["NullableInt"] = new() { NULL = true }, // null int?

    // Long
    ["LongValue"] = new() { N = "9223372036854775807" },
    ["NullableLong"] = new() { N = "123456789012345" },
    ["NullLong"] = new() { NULL = true },

    // Double
    ["Price"] = new() { N = "19.99" },
    ["Temperature"] = new() { N = "-273.15" },
    ["NullableDouble"] = new() { N = "3.14159" },
    ["NullDouble"] = new() { NULL = true },

    // Decimal
    ["Amount"] = new() { N = "1234567.89" },
    ["Precision"] = new() { N = "0.000000001" },
    ["NullableDecimal"] = new() { N = "999.99" },
    ["NullDecimal"] = new() { NULL = true },

    // Guid
    ["UserId"] = new() { S = "a1b2c3d4-e5f6-7890-abcd-ef1234567890" },
    ["CorrelationId"] = new() { S = "00000000-0000-0000-0000-000000000000" }, // Guid.Empty

    // Nullable Guid
    ["OptionalGuid"] = new() { S = "12345678-1234-1234-1234-123456789012" },
    ["NullGuid"] = new() { NULL = true },

    // DateTime (ISO-8601)
    ["CreatedAt"] = new() { S = "2025-12-22T10:30:00.0000000Z" },
    ["UpdatedAt"] = new() { S = "2025-12-22T14:45:30.1234567Z" },
    ["LocalDateTime"] = new() { S = "2025-12-22T10:30:00.0000000" }, // without Z for local

    // Nullable DateTime
    ["DeletedAt"] = new() { S = "2025-12-20T08:00:00.0000000Z" },
    ["NullDateTime"] = new() { NULL = true },

    // DateTimeOffset (ISO-8601 with offset)
    ["EventTime"] = new() { S = "2025-12-22T10:30:00.0000000+00:00" },
    ["ScheduledTime"] = new() { S = "2025-12-22T14:30:00.0000000-05:00" },

    // Nullable DateTimeOffset
    ["CompletedAt"] = new() { S = "2025-12-22T12:00:00.0000000+01:00" },
    ["NullDateTimeOffset"] = new() { NULL = true },

    // TimeSpan (ISO-8601 Duration format: PT{hours}H{minutes}M{seconds}S)
    ["Duration"] = new() { S = "PT2H30M45S" }, // 2 hours, 30 minutes, 45 seconds
    ["ShortDuration"] = new() { S = "PT15M" }, // 15 minutes
    ["LongDuration"] = new() { S = "P1DT12H" }, // 1 day, 12 hours
    ["NegativeDuration"] = new() { S = "-PT1H30M" }, // -1.5 hours

    // Alternative: TimeSpan as Ticks (if you prefer ticks over ISO-8601 duration)
    ["DurationTicks"] = new() { N = "90450000000" }, // 2h 30m 45s in ticks

    // Nullable TimeSpan
    ["OptionalDuration"] = new() { S = "PT5H" },
    ["NullTimeSpan"] = new() { NULL = true },

    // Enum (stored as string name)
    ["Status"] = new() { S = "Active" },
    ["Priority"] = new() { S = "High" },
    ["OrderStatus"] = new() { S = "Pending" },
};

var myEntity = ExampleEntityMapper.FromItem(exampleAttributes);

[DynamoMapper]
public static class ExampleEntityMapper
{
    public static Dictionary<string, AttributeValue> ToItem(ExampleEntity source) =>
        new()
        {
            ["Age"] = new AttributeValue { N = source.Age.ToString(CultureInfo.InvariantCulture) },
            ["Amount"] = new AttributeValue
            {
                N = source.Amount.ToString(CultureInfo.InvariantCulture),
            },
            ["CompletedAt"] = source.CompletedAt.HasValue
                ? new AttributeValue
                {
                    S = source.CompletedAt.Value.ToString("O", CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["CorrelationId"] = new AttributeValue { S = source.CorrelationId.ToString() },
            ["Count"] = new AttributeValue
            {
                N = source.Count.ToString(CultureInfo.InvariantCulture),
            },
            ["CreatedAt"] = new AttributeValue
            {
                S = source.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
            },
            ["DeletedAt"] = source.DeletedAt.HasValue
                ? new AttributeValue
                {
                    S = source.DeletedAt.Value.ToString("O", CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["Duration"] = new AttributeValue
            {
                S = source.Duration.ToString("c", CultureInfo.InvariantCulture),
            },
            ["DurationTicks"] = new AttributeValue
            {
                N = source.DurationTicks.Ticks.ToString(CultureInfo.InvariantCulture),
            },
            ["EmptyString"] = new AttributeValue { S = source.EmptyString ?? string.Empty },
            ["EventTime"] = new AttributeValue
            {
                S = source.EventTime.ToString("O", CultureInfo.InvariantCulture),
            },
            ["IsActive"] = new AttributeValue { BOOL = source.IsActive },
            ["IsDeleted"] = new AttributeValue { BOOL = source.IsDeleted },
            ["IsOptional"] = source.IsOptional.HasValue
                ? new AttributeValue { BOOL = source.IsOptional.Value }
                : new AttributeValue { NULL = true },
            ["IsVerified"] = source.IsVerified.HasValue
                ? new AttributeValue { BOOL = source.IsVerified.Value }
                : new AttributeValue { NULL = true },
            ["LocalDateTime"] = new AttributeValue
            {
                S = source.LocalDateTime.ToString("O", CultureInfo.InvariantCulture),
            },
            ["LongDuration"] = new AttributeValue
            {
                S = source.LongDuration.ToString("c", CultureInfo.InvariantCulture),
            },
            ["LongValue"] = new AttributeValue
            {
                N = source.LongValue.ToString(CultureInfo.InvariantCulture),
            },
            ["Name"] = new AttributeValue { S = source.Name ?? string.Empty },
            ["NegativeDuration"] = new AttributeValue
            {
                S = source.NegativeDuration.ToString("c", CultureInfo.InvariantCulture),
            },
            ["NegativeInt"] = new AttributeValue
            {
                N = source.NegativeInt.ToString(CultureInfo.InvariantCulture),
            },
            ["NullableDecimal"] = source.NullableDecimal.HasValue
                ? new AttributeValue
                {
                    N = source.NullableDecimal.Value.ToString(CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullableDouble"] = source.NullableDouble.HasValue
                ? new AttributeValue
                {
                    N = source.NullableDouble.Value.ToString(CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullableInt"] = source.NullableInt.HasValue
                ? new AttributeValue
                {
                    N = source.NullableInt.Value.ToString(CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullableLong"] = source.NullableLong.HasValue
                ? new AttributeValue
                {
                    N = source.NullableLong.Value.ToString(CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullDateTime"] = source.NullDateTime.HasValue
                ? new AttributeValue
                {
                    S = source.NullDateTime.Value.ToString("O", CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullDateTimeOffset"] = source.NullDateTimeOffset.HasValue
                ? new AttributeValue
                {
                    S = source.NullDateTimeOffset.Value.ToString("O", CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullDecimal"] = source.NullDecimal.HasValue
                ? new AttributeValue
                {
                    N = source.NullDecimal.Value.ToString(CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullDouble"] = source.NullDouble.HasValue
                ? new AttributeValue
                {
                    N = source.NullDouble.Value.ToString(CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullGuid"] = source.NullGuid.HasValue
                ? new AttributeValue { S = source.NullGuid.Value.ToString() }
                : new AttributeValue { NULL = true },
            ["NullLong"] = source.NullLong.HasValue
                ? new AttributeValue
                {
                    N = source.NullLong.Value.ToString(CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["NullTimeSpan"] = source.NullTimeSpan.HasValue
                ? new AttributeValue
                {
                    S = source.NullTimeSpan.Value.ToString("c", CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["OptionalCount"] = source.OptionalCount.HasValue
                ? new AttributeValue
                {
                    N = source.OptionalCount.Value.ToString(CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["OptionalDuration"] = source.OptionalDuration.HasValue
                ? new AttributeValue
                {
                    S = source.OptionalDuration.Value.ToString("c", CultureInfo.InvariantCulture),
                }
                : new AttributeValue { NULL = true },
            ["OptionalGuid"] = source.OptionalGuid.HasValue
                ? new AttributeValue { S = source.OptionalGuid.Value.ToString() }
                : new AttributeValue { NULL = true },
            ["OrderStatus"] = new AttributeValue { S = source.OrderStatus.ToString() },
            ["Precision"] = new AttributeValue
            {
                N = source.Precision.ToString(CultureInfo.InvariantCulture),
            },
            ["Price"] = new AttributeValue
            {
                N = source.Price.ToString(CultureInfo.InvariantCulture),
            },
            ["Priority"] = new AttributeValue { S = source.Priority.ToString() },
            ["ScheduledTime"] = new AttributeValue
            {
                S = source.ScheduledTime.ToString("O", CultureInfo.InvariantCulture),
            },
            ["ShortDuration"] = new AttributeValue
            {
                S = source.ShortDuration.ToString("c", CultureInfo.InvariantCulture),
            },
            ["Status"] = new AttributeValue { S = source.Status.ToString() },
            ["Temperature"] = new AttributeValue
            {
                N = source.Temperature.ToString(CultureInfo.InvariantCulture),
            },
            ["UpdatedAt"] = new AttributeValue
            {
                S = source.UpdatedAt.ToString("O", CultureInfo.InvariantCulture),
            },
            ["UserId"] = new AttributeValue { S = source.UserId.ToString() },
        };

    public static ExampleEntity FromItem(Dictionary<string, AttributeValue> item)
    {
        var entity = new ExampleEntity
        {
            Age = item.GetInt("Age"),
            Amount = item.GetDecimal("Amount"),
            CompletedAt = item.GetNullableDateTimeOffset("CompletedAt"),
            CorrelationId = item.GetGuid("CorrelationId"),
            Count = item.GetInt("Count"),
            CreatedAt = item.GetDateTime("CreatedAt"),
            DeletedAt = item.GetNullableDateTime("DeletedAt"),
            Duration = item.GetTimeSpan("Duration"),
            DurationTicks = TimeSpan.FromTicks(item.GetLong("DurationTicks")),
            EmptyString = item.GetString("EmptyString"),
            EventTime = item.GetDateTimeOffset("EventTime"),
            IsActive = item.GetBool("IsActive"),
            IsDeleted = item.GetBool("IsDeleted"),
            IsOptional = item.GetNullableBool("IsOptional"),
            IsVerified = item.GetNullableBool("IsVerified"),
            LocalDateTime = item.GetDateTime("LocalDateTime"),
            LongDuration = item.GetTimeSpan("LongDuration"),
            LongValue = item.GetLong("LongValue"),
            Name = item.GetString("Name"),
            NegativeDuration = item.GetTimeSpan("NegativeDuration"),
            NegativeInt = item.GetInt("NegativeInt"),
            NullableDecimal = item.GetNullableDecimal("NullableDecimal"),
            NullableDouble = item.GetNullableDouble("NullableDouble"),
            NullableInt = item.GetNullableInt("NullableInt"),
            NullableLong = item.GetNullableLong("NullableLong"),
            NullDateTime = item.GetNullableDateTime("NullDateTime"),
            NullDateTimeOffset = item.GetNullableDateTimeOffset("NullDateTimeOffset"),
            NullDecimal = item.GetNullableDecimal("NullDecimal"),
            NullDouble = item.GetNullableDouble("NullDouble"),
            NullGuid = item.GetNullableGuid("NullGuid"),
            NullLong = item.GetNullableLong("NullLong"),
            NullTimeSpan = item.GetNullableTimeSpan("NullTimeSpan"),
            OptionalCount = item.GetNullableInt("OptionalCount"),
            OptionalDuration = item.GetNullableTimeSpan("OptionalDuration"),
            OptionalGuid = item.GetNullableGuid("OptionalGuid"),
            OrderStatus = item.GetEnum("OrderStatus", OrderStatus.Pending),
            Precision = item.GetDecimal("Precision"),
            Price = item.GetDouble("Price"),
            Priority = item.GetEnum("Priority", Priority.Low),
            ScheduledTime = item.GetDateTimeOffset("ScheduledTime"),
            ShortDuration = item.GetTimeSpan("ShortDuration"),
            Status = item.GetEnum("Status", Status.Active),
            Temperature = item.GetDouble("Temperature"),
            UpdatedAt = item.GetDateTime("UpdatedAt"),
            UserId = item.GetGuid("UserId"),
        };

        return entity;
    }
}

// POCO with properties matching the dictionary
public class ExampleEntity
{
    // Integer types
    public int Age { get; set; }

    // Decimal
    public decimal Amount { get; set; }

    // Nullable DateTimeOffset
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid CorrelationId { get; set; }
    public int Count { get; set; }

    // DateTime (ISO-8601)
    public DateTime CreatedAt { get; set; }

    // Nullable DateTime
    public DateTime? DeletedAt { get; set; }

    // TimeSpan (ISO-8601 Duration format)
    public TimeSpan Duration { get; set; }
    public TimeSpan DurationTicks { get; set; }
    public string EmptyString { get; set; } = string.Empty;

    // DateTimeOffset (ISO-8601 with offset)
    public DateTimeOffset EventTime { get; set; }

    // Boolean
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public bool? IsOptional { get; set; }

    // Nullable Boolean
    public bool? IsVerified { get; set; }
    public DateTime LocalDateTime { get; set; }
    public TimeSpan LongDuration { get; set; }

    // Long
    public long LongValue { get; set; }

    // String
    public string Name { get; set; } = string.Empty;
    public TimeSpan NegativeDuration { get; set; }
    public int NegativeInt { get; set; }
    public decimal? NullableDecimal { get; set; }
    public double? NullableDouble { get; set; }
    public int? NullableInt { get; set; }
    public long? NullableLong { get; set; }
    public DateTime? NullDateTime { get; set; }
    public DateTimeOffset? NullDateTimeOffset { get; set; }
    public decimal? NullDecimal { get; set; }
    public double? NullDouble { get; set; }
    public Guid? NullGuid { get; set; }
    public long? NullLong { get; set; }
    public TimeSpan? NullTimeSpan { get; set; }

    // Nullable int
    public int? OptionalCount { get; set; }

    // Nullable TimeSpan
    public TimeSpan? OptionalDuration { get; set; }

    // Nullable Guid
    public Guid? OptionalGuid { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public decimal Precision { get; set; }

    // Double
    public double Price { get; set; }
    public Priority Priority { get; set; }
    public DateTimeOffset ScheduledTime { get; set; }
    public TimeSpan ShortDuration { get; set; }

    // Enum (stored as string name)
    public Status? Status { get; set; }
    public double Temperature { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UserId { get; set; }
}

// Example enum definitions for reference
public enum Status
{
    Active,
    Inactive,
    Suspended,
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical,
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
}

public static partial class MyThing
{
    public static partial void DoStuff(int int1);
}

public static partial class MyThing
{
    public static partial void DoStuff(int x) { }
}
