using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

/// <summary>
/// Example: Mapper-based nested object - Author has its own mapper
/// </summary>
public record BlogPost
{
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required Author Writer { get; set; }
}

public record Author
{
    public required string Handle { get; set; }
    public required string DisplayName { get; set; }
    public required string Bio { get; set; }
}

// Author has its own mapper - BlogPostMapper will use this
[DynamoMapper]
public static partial class AuthorMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Author source);

    public static partial Author FromItem(Dictionary<string, AttributeValue> item);
}

[DynamoMapper]
public static partial class BlogPostMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(BlogPost source);

    public static partial BlogPost FromItem(Dictionary<string, AttributeValue> item);
}
