namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class DynamoIgnoreVerifyTests
{
    [Fact]
    public async Task DynamoIgnore_Simple() =>
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
                [DynamoIgnore(nameof(ExampleEntity.Ignore))]
                [DynamoIgnore(nameof(ExampleEntity.IgnoreAll), Ignore = IgnoreMapping.All)]
                [DynamoIgnore(nameof(ExampleEntity.IgnoreInFromModel), Ignore = IgnoreMapping.FromModel)]
                [DynamoIgnore(nameof(ExampleEntity.IgnoreInToModel), Ignore = IgnoreMapping.ToModel)]
                internal static partial class ExampleEntityMapper
                {
                    internal static partial Dictionary<string, AttributeValue> FromModel(ExampleEntity source);

                    internal static partial ExampleEntity ToModel(Dictionary<string, AttributeValue> item);
                }

                internal class ExampleEntity
                {
                    internal required string String { get; set; }
                    internal Type Ignore { get; set; } = typeof(ExampleEntity);
                    internal string IgnoreAll { get; set; } = string.Empty;
                    internal string IgnoreInFromModel { get; set; } = string.Empty;
                    internal string IgnoreInToModel { get; set; } = string.Empty;
                }
                """,
            },
            TestContext.Current.CancellationToken
        );
}
