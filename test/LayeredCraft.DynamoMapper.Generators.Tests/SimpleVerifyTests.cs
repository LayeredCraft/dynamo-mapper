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

                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> item);
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
    public async Task Simple_Record() =>
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

                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> item);
                }

                public record MyDto
                {
                    public string Name { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_EmptyModel() =>
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

                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> item);
                }

                public class MyDto
                {
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
                    public bool Bool { get; set; }
                    public bool? NullableBool { get; set; }
                    public DateTime DateTime { get; set; }
                    public DateTime? NullableDateTime { get; set; }
                    public DateTimeOffset DateTimeOffset { get; set; }
                    public DateTimeOffset? NullableDateTimeOffset { get; set; }
                    public decimal Decimal { get; set; }
                    public decimal? NullableDecimal { get; set; }
                    public double Double { get; set; }
                    public double? NullableDouble { get; set; }
                    public Guid Guid { get; set; }
                    public Guid? NullableGuid { get; set; }
                    public int Int { get; set; }
                    public int? NullableInt { get; set; }
                    public long Long { get; set; }
                    public long? NullableLong { get; set; }
                    public string String { get; set; }
                    public string? NullableString { get; set; }
                    public TimeSpan TimeSpan { get; set; }
                    public TimeSpan? NullableTimeSpan { get; set; }
                    public OrderStatus Enum { get; set; }
                    public OrderStatus? NullableEnum { get; set; }
                }

                public enum OrderStatus
                {
                    Pending,
                    Processing,
                    Shipped,
                    Delivered,
                    Cancelled,
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
                    EnumFormat = "G"
                )]
                public static partial class ExampleEntityMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    public static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> x);
                }

                public class ExampleEntity
                {
                    public bool Bool { get; set; }
                    public bool? NullableBool { get; set; }
                    public DateTime DateTime { get; set; }
                    public DateTime? NullableDateTime { get; set; }
                    public DateTimeOffset DateTimeOffset { get; set; }
                    public DateTimeOffset? NullableDateTimeOffset { get; set; }
                    public decimal Decimal { get; set; }
                    public decimal? NullableDecimal { get; set; }
                    public double Double { get; set; }
                    public double? NullableDouble { get; set; }
                    public Guid Guid { get; set; }
                    public Guid? NullableGuid { get; set; }
                    public int Int { get; set; }
                    public int? NullableInt { get; set; }
                    public long Long { get; set; }
                    public long? NullableLong { get; set; }
                    public string String { get; set; }
                    public string? NullableString { get; set; }
                    public TimeSpan TimeSpan { get; set; }
                    public TimeSpan? NullableTimeSpan { get; set; }
                    public OrderStatus Enum { get; set; }
                    public OrderStatus? NullableEnum { get; set; }
                }

                public enum OrderStatus
                {
                    Pending,
                    Processing,
                    Shipped,
                    Delivered,
                    Cancelled,
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_PropertiesWithNoSetter() =>
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

                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> item);
                }

                public class MyDto
                {
                    public string Name { get; set; }
                    public string ReadOnlyString { get; } = "ReadOnlyString";
                    public string ExpressionProperty => "ExpressionProperty";
                }
                """,
            },
            TestContext.Current.CancellationToken
        );
}
