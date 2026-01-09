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
                [DynamoField(nameof(ExampleEntity.String), AttributeName = "customName")]
                [DynamoField(nameof(ExampleEntity.NullableString), AttributeName = "ANOTHER_NAME")]
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
}
