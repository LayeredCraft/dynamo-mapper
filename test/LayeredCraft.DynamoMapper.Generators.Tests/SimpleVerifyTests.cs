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
    public async Task Simple_AllHelperTypes_Optional() =>
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
                    public bool Bool { get; set; } = false;
                    public bool? NullableBool { get; set; } = null;
                    public DateTime DateTime { get; set; } = DateTime.MinValue;
                    public DateTime? NullableDateTime { get; set; } = null;
                    public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.MinValue;
                    public DateTimeOffset? NullableDateTimeOffset { get; set; } = null;
                    public decimal Decimal { get; set; } = 0m;
                    public decimal? NullableDecimal { get; set; } = null;
                    public double Double { get; set; } = 0d;
                    public double? NullableDouble { get; set; } = null;
                    public Guid Guid { get; set; } = Guid.Empty;
                    public Guid? NullableGuid { get; set; } = null;
                    public int Int { get; set; } = 0;
                    public int? NullableInt { get; set; } = null;
                    public long Long { get; set; } = 0L;
                    public long? NullableLong { get; set; } = null;
                    public string String { get; set; } = string.Empty;
                    public string? NullableString { get; set; } = null;
                    public TimeSpan TimeSpan { get; set; } = TimeSpan.Zero;
                    public TimeSpan? NullableTimeSpan { get; set; } = null;
                    public OrderStatus Enum { get; set; } = OrderStatus.Pending;
                    public OrderStatus? NullableEnum { get; set; } = null;
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

    [Fact]
    public async Task Simple_NoToMethod() =>
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
                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> item);
                }

                public class MyDto
                {
                    public string Name { get; set; }
                    public Type ShouldNotBeMapped => typeof(MyDto);
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_OverrideOnlyOnFromMethod_NoToMethod() =>
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
                [DynamoField(nameof(MyDto.ShouldNotBeMapped), FromMethod = nameof(GetType))]
                public static partial class ExampleMyDtoMapper
                {
                    public static partial MyDto FromItem(Dictionary<string, AttributeValue> item);

                    public static Type GetType(Dictionary<string, AttributeValue> item)
                    {
                        return typeof(MyDto);
                    }
                }

                public class MyDto
                {
                    public required string Name { get; set; }
                    public Type ShouldNotBeMapped { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_OverrideOnlyOnToMethod_NoFromMethod() =>
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
                [DynamoField(nameof(MyDto.ShouldNotBeMapped), ToMethod = nameof(SetType))]
                public static partial class ExampleMyDtoMapper
                {
                    public static partial Dictionary<string, AttributeValue> ToItem(MyDto source);

                    public static AttributeValue SetType(MyDto source)
                    {
                        return new AttributeValue();
                    }
                }

                public class MyDto
                {
                    public required string Name { get; set; }
                    public Type ShouldNotBeMapped { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Simple_DefaultValues() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                [DynamoMapper]
                internal static partial class UserMapper
                {
                    internal static partial User FromItem(Dictionary<string, AttributeValue> item);

                    internal static partial Dictionary<string, AttributeValue> ToItem(User user);
                }

                public class User
                {
                    public required string FirstName { get; init; } = string.Empty;
                    public string MiddleName { get; set; } = string.Empty;
                    public required string LastName { get; set; }
                    public int Age { get; set; }
                    public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
                    public string FullName => $"{FirstName} {LastName}";
                }
                """,
            },
            TestContext.Current.CancellationToken
        );
}
