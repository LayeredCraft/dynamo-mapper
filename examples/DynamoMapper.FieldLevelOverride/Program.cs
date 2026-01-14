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

var myEntity = ExampleDtoMapper.FromItem(exampleAttributes);

[DynamoMapper]
[DynamoField(
    nameof(ExampleDto.String),
    AttributeName = "customName",
    ToMethod = nameof(CustomValueToAttribute),
    FromMethod = nameof(CustomValueFromAttribute)
)]
[DynamoField(
    nameof(ExampleDto.NullableString),
    AttributeName = "ANOTHER_NAME",
    Required = true,
    Kind = DynamoKind.N,
    OmitIfNull = false,
    OmitIfEmptyString = true
)]
internal static partial class ExampleDtoMapper
{
    internal static partial Dictionary<string, AttributeValue> ToItem(ExampleDto source);

    internal static partial ExampleDto FromItem(Dictionary<string, AttributeValue> item);

    internal static AttributeValue CustomValueToAttribute(ExampleDto source) =>
        // custom logic here before returning an attribute value
        new() { S = source.String };

    internal static string CustomValueFromAttribute(Dictionary<string, AttributeValue> item)
    {
        var value = item["customName"].S;
        // custom logic here before returning a string
        return value;
    }
}

internal class ExampleDto
{
    internal required string String { get; set; }
    internal string? NullableString { get; set; }
}
