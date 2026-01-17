namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class DynamoFieldFormatVerifyTests
{
    [Fact]
    public async Task DateTime_WithCustomFormat() =>
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
                [DynamoField(nameof(Product.CreatedAt), Format = "yyyy-MM-dd")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required DateTime CreatedAt { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DateTimeOffset_WithCustomFormat() =>
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
                [DynamoField(nameof(Product.CreatedAt), Format = "yyyy-MM-ddTHH:mm:ssK")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required DateTimeOffset CreatedAt { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Guid_WithNFormat() =>
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
                [DynamoField(nameof(Product.Id), Format = "N")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required Guid Id { get; set; }
                    internal required string Name { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task TimeSpan_WithGeneralLongFormat() =>
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
                [DynamoField(nameof(Product.Duration), Format = "G")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required TimeSpan Duration { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task Enum_WithDecimalFormat() =>
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
                [DynamoField(nameof(Product.Status), Format = "D")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required ProductStatus Status { get; set; }
                }

                internal enum ProductStatus
                {
                    Active,
                    Inactive,
                    Archived
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task MultiplePropertiesWithDifferentFormats() =>
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
                [DynamoField(nameof(Product.Id), Format = "N")]
                [DynamoField(nameof(Product.CreatedAt), Format = "yyyy-MM-dd")]
                [DynamoField(nameof(Product.Duration), Format = "G")]
                [DynamoField(nameof(Product.Status), Format = "D")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required Guid Id { get; set; }
                    internal required string Name { get; set; }
                    internal required DateTime CreatedAt { get; set; }
                    internal required TimeSpan Duration { get; set; }
                    internal required ProductStatus Status { get; set; }
                }

                internal enum ProductStatus
                {
                    Active,
                    Inactive,
                    Archived
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task FormatOverrideWithMapperDefaults() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(DateTimeFormat = "O", GuidFormat = "D", TimeSpanFormat = "c", EnumFormat = "G")]
                [DynamoField(nameof(Product.Id), Format = "N")]
                [DynamoField(nameof(Product.CreatedAt), Format = "yyyy-MM-dd")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required Guid Id { get; set; }
                    internal required string Name { get; set; }
                    internal required DateTime CreatedAt { get; set; }
                    internal required TimeSpan Duration { get; set; }
                    internal required ProductStatus Status { get; set; }
                }

                internal enum ProductStatus
                {
                    Active,
                    Inactive,
                    Archived
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NonFormattableTypeWithFormat_IsIgnored() =>
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
                [DynamoField(nameof(Product.Name), Format = "custom-format")]
                [DynamoField(nameof(Product.Price), Format = "N2")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required decimal Price { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task NullableTypesWithFormat() =>
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
                [DynamoField(nameof(Product.Id), Format = "N")]
                [DynamoField(nameof(Product.CreatedAt), Format = "yyyy-MM-dd")]
                [DynamoField(nameof(Product.Duration), Format = "G")]
                [DynamoField(nameof(Product.Status), Format = "D")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal Guid? Id { get; set; }
                    internal DateTime? CreatedAt { get; set; }
                    internal TimeSpan? Duration { get; set; }
                    internal ProductStatus? Status { get; set; }
                }

                internal enum ProductStatus
                {
                    Active,
                    Inactive,
                    Archived
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task AllFormattableTypes_SingleMapper() =>
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
                [DynamoField(nameof(Product.Id), Format = "N")]
                [DynamoField(nameof(Product.CreatedAt), Format = "yyyy-MM-dd")]
                [DynamoField(nameof(Product.UpdatedAt), Format = "yyyy-MM-ddTHH:mm:ssK")]
                [DynamoField(nameof(Product.Duration), Format = "G")]
                [DynamoField(nameof(Product.Status), Format = "D")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required Guid Id { get; set; }
                    internal required string Name { get; set; }
                    internal required DateTime CreatedAt { get; set; }
                    internal required DateTimeOffset UpdatedAt { get; set; }
                    internal required TimeSpan Duration { get; set; }
                    internal required ProductStatus Status { get; set; }
                }

                internal enum ProductStatus
                {
                    Active,
                    Inactive,
                    Archived
                }
                """,
            },
            TestContext.Current.CancellationToken
        );
}
