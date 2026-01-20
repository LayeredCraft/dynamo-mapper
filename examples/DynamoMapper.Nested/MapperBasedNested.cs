using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

/// <summary>
/// Example: Mapper-based nested object - Author has its own mapper
/// </summary>
public record BlogPost
{
    public string Slug { get; set; }
    public string Title { get; set; }
    public Author Writer { get; set; }
}

public record Author
{
    public string Handle { get; set; }
    public string DisplayName { get; set; }
    public string Bio { get; set; }
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