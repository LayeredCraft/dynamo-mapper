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
[DynamoField(
    nameof(ExampleEntity.String),
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

    internal static AttributeValue ToMethod(string value) => new() { S = value };

    internal static string FromMethod(Dictionary<string, AttributeValue> item) =>
        item["customName"].S;
}

internal class ExampleEntity
{
    internal required string String { get; set; }
    internal string? NullableString { get; set; }
}
