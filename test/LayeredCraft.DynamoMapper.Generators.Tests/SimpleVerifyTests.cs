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
}
