namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class DateOnlyTimeOnlyVerifyTests
{
    [Fact]
    public async Task DateOnly_AsRootScalar() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required DateOnly StartDate { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task NullableDateOnly_AsRootScalar() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal DateOnly? StartDate { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task TimeOnly_AsRootScalar() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required TimeOnly StartTime { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task NullableTimeOnly_AsRootScalar() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal TimeOnly? StartTime { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task DateOnly_InList() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required List<DateOnly> AvailableDates { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task TimeOnly_InList() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required List<TimeOnly> AvailableTimes { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task DateOnly_WithCustomFormat() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(Product.StartDate), Format = "yyyyMMdd")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required DateOnly StartDate { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task TimeOnly_WithCustomFormat() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(Product.StartTime), Format = "HH:mm")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required TimeOnly StartTime { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );

    [Fact]
    public async Task DateOnly_WithMapperDefaultFormat() => await GeneratorTestHelpers.Verify(
        new VerifyTestOptions
        {
            SourceCode =
                """
                using System;
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using LayeredCraft.DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper(DateOnlyFormat = "yyyyMMdd")]
                internal static partial class ProductMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(Product source);
                    internal static partial Product FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class Product
                {
                    internal required string Name { get; set; }
                    internal required DateOnly StartDate { get; set; }
                }
                """,
        },
        TestContext.Current.CancellationToken
    );
}
