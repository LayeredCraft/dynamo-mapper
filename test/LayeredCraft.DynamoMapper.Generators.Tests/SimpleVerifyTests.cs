namespace LayeredCraft.DynamoMapper.Generators.Tests;

public class SimpleVerifyTests
{
    [Fact]
    public async Task Simple_HelloWorld() =>
        await GeneratorTestHelpers.Verify(
            new VerifyTestOptions
            {
                SourceCode = """
                using Amazon.DynamoDBv2.Model;
                using DynamoMapper.Runtime;
                using System.Collections.Generic;

                [DynamoMapper]
                public static partial class ExampleEntityMapper
                {
                    // public static partial Dictionary<string, AttributeValue> ToItem(string source);

                    // public static partial string FromItem(Dictionary<string, AttributeValue> item);
                }

                [DynamoMapper]
                public static partial class ExampleEntityMapper2
                {
                    // public static partial Dictionary<string, AttributeValue> ToItem(string source);

                    // public static partial string FromItem(Dictionary<string, AttributeValue> item);
                }
                """,
            },
            TestContext.Current.CancellationToken
        );
}
