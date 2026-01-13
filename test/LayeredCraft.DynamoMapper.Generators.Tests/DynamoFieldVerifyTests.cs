namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class DynamoFieldVerifyTests
{
    [Fact]
    public async Task DynamoField_Simple() =>
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
                [DynamoField(
                    nameof(ExampleEntity.String),
                    AttributeName = "customName",
                    ToMethod = nameof(ToMethod),
                    FromMethod = nameof(FromMethod)
                )]
                [DynamoField(
                    nameof(ExampleEntity.NullableString),
                    AttributeName = "ANOTHER_NAME",
                    Required = true,
                    Kind = DynamoKind.N,
                    OmitIfNull = false,
                    OmitIfEmptyString = true
                )]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);

                    internal static AttributeValue ToMethod(ExampleEntity source) => new() { S = source.String };

                    internal static string FromMethod(Dictionary<string, AttributeValue> item) =>
                        item["customName"].S;
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DynamoField_NullableFieldRequired() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(ExampleEntity.NullableString), Required = true)]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DynamoField_NotNullableOptional() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(ExampleEntity.String), Required = false)]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DynamoField_KindOverride() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(ExampleEntity.String), Kind = DynamoKind.B)]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DynamoField_OmitIfNullOverride() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(ExampleEntity.String), OmitIfNull = false)]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DynamoField_OmitIfEmptyStringOverride() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(ExampleEntity.String), OmitIfEmptyString = true)]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DynamoField_ToMethod() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(ExampleEntity.String), ToMethod = nameof(ToMethod))]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);

                    internal static AttributeValue ToMethod(ExampleEntity source) => new() { S = source.String };
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                }

                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DynamoField_FromMethod() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(nameof(ExampleEntity.String), FromMethod = nameof(FromMethod))]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);

                    internal static string FromMethod(Dictionary<string, AttributeValue> item) =>
                        item["customName"].S;
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                }
                """,
            },
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task DynamoField_FromAndToMethod_CustomType() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using System.Collections.Generic;
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;

                namespace MyNamespace;

                [DynamoMapper]
                [DynamoField(
                    nameof(ExampleEntity.String),
                    FromMethod = nameof(PersonFromAttr),
                    ToMethod = nameof(PersonToAttr)
                )]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleEntity source);

                    internal static partial ExampleEntity FromItem(Dictionary<string, AttributeValue> item);

                    internal static Person PersonFromAttr(Dictionary<string, AttributeValue> item) =>
                        new(item["customName"].S);

                    internal static AttributeValue PersonToAttr(ExampleEntity source) =>
                        new() { S = source.Person.name };
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal string? NullableString { get; set; }
                    internal Person Person { get; set; }
                }

                internal record Person(string name);
                """,
            },
            TestContext.Current.CancellationToken
        );
}
