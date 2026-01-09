using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

// Example enum definitions
var exampleAttributes = new Dictionary<string, AttributeValue>
{
    // string
    ["customName"] = new() { S = "John Doe" },

    // string?
    ["ANOTHER_NAME"] = new() { S = "Optional text" },
};

var myEntity = ExampleEntityMapper.FromItem(exampleAttributes);

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
