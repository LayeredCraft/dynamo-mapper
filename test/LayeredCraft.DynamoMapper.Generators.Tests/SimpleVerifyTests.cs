namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class SimpleVerifyTests
{
    [Fact]
    public async Task Simple_HelloWorld() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(MyDto source);

                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> x);
                }

                public class MyDto
                {
                    public string Name { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_AllHelperTypes() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    public static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> x);
                }

                public class ExampleEntity
                {
                    public int Age { get; set; }
                    public decimal Amount { get; set; }
                    public DateTimeOffset? CompletedAt { get; set; }
                    public Guid CorrelationId { get; set; }
                    public DateTime CreatedAt { get; set; }
                    public DateTime? DeletedAt { get; set; }
                    public TimeSpan Duration { get; set; }
                    public string EmptyString { get; set; }
                    public DateTimeOffset EventTime { get; set; }
                    public bool IsDeleted { get; set; }
                    public bool? IsOptional { get; set; }
                    public long LongValue { get; set; }
                    public string? Name { get; set; }
                    public decimal? NullableDecimal { get; set; }
                    public double? NullableDouble { get; set; }
                    public int? NullableInt { get; set; }
                    public long? NullableLong { get; set; }
                    public Guid? NullGuid { get; set; }
                    public TimeSpan? NullTimeSpan { get; set; }
                    public OrderStatus OrderStatus { get; set; }
                    public double Price { get; set; }
                    public Status? Status { get; set; }
                }

                public enum OrderStatus
                {
                    Pending,
                    Processing,
                    Shipped,
                    Delivered,
                    Cancelled,
                }

                public enum Status
                {
                    Active,
                    Inactive,
                    Suspended,
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_InitProperty() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(MyDto source);

                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> x);
                }

                public class MyDto
                {
                    public string Name { get; init; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_NoSetter() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(MyDto source);

                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> x);
                }

                public class MyDto
                {
                    public string Name { get; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_MethodNamePrefixWorks() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToAttributeValues(MyDto source);

                    public static partial MyDto FromAttributeValues(Dictionary<string, AttributeValue> x);
                }

                public class MyDto
                {
                    public string Name { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_AllOptionsSetToNonDefaultValues() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(
                    Convention = DynamoNamingConvention.SnakeCase,
                    DefaultRequiredness = Requiredness.Required,
                    OmitNullStrings = false,
                    OmitEmptyStrings = true,
                    DateTimeFormat = "I",
                    EnumFormat = EnumFormat.Numeric
                )]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    public static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> x);
                }

                public class ExampleEntity
                {
                    public int Age { get; set; }
                    public decimal Amount { get; set; }
                    public DateTimeOffset? CompletedAt { get; set; }
                    public Guid CorrelationId { get; set; }
                    public DateTime CreatedAt { get; set; }
                    public DateTime? DeletedAt { get; set; }
                    public TimeSpan Duration { get; set; }
                    public string EmptyString { get; set; }
                    public DateTimeOffset EventTime { get; set; }
                    public bool IsDeleted { get; set; }
                    public bool? IsOptional { get; set; }
                    public long LongValue { get; set; }
                    public string? Name { get; set; }
                    public decimal? NullableDecimal { get; set; }
                    public double? NullableDouble { get; set; }
                    public int? NullableInt { get; set; }
                    public long? NullableLong { get; set; }
                    public Guid? NullGuid { get; set; }
                    public TimeSpan? NullTimeSpan { get; set; }
                    public OrderStatus OrderStatus { get; set; }
                    public double Price { get; set; }
                    public Status? Status { get; set; }
                }

                public enum OrderStatus
                {
                    Pending,
                    Processing,
                    Shipped,
                    Delivered,
                    Cancelled,
                }

                public enum Status
                {
                    Active,
                    Inactive,
                    Suspended,
                }
                """,
            },
            TestContext.Current.CancellationToken
        );
}
